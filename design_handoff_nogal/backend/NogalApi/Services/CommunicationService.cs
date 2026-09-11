using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Models.Communication;

namespace NogalApi.Services;

public class CommunicationService : ICommunicationService
{
    private readonly AppDbContext _context;

    public CommunicationService(AppDbContext context) => _context = context;

    public async Task CrearContactoAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default)
    {
        var nombre = dto.Nombre.Trim();
        var contacto = dto.Contacto.Trim();
        var mensaje = dto.Mensaje.Trim();
        if (nombre.Length == 0 || contacto.Length == 0 || mensaje.Length == 0)
            throw new CommunicationValidationException("Nombre, contacto y mensaje son obligatorios.");
        if (nombre.Length > 120 || contacto.Length > 120)
            throw new CommunicationValidationException("Nombre y contacto pueden tener máximo 120 caracteres.");

        _context.ContactMessages.Add(new ContactMessage { Nombre = nombre, Contacto = contacto, Mensaje = mensaje });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatResponseDto> ResponderChatAsync(ChatRequestDto dto, CancellationToken cancellationToken = default)
    {
        var texto = dto.Mensaje.Trim();
        if (texto.Length == 0) throw new CommunicationValidationException("Escribe un mensaje para continuar.");
        if (texto.Length > 2000) throw new CommunicationValidationException("El mensaje puede tener máximo 2.000 caracteres.");

        var sessionId = Guid.TryParse(dto.SessionId, out var sessionGuid) ? sessionGuid.ToString() : Guid.NewGuid().ToString();
        var sesion = await _context.ChatSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        if (sesion is null)
        {
            sesion = new ChatSession { SessionId = sessionId };
            _context.ChatSessions.Add(sesion);
        }

        var respuesta = await CrearRespuestaAsync(texto, cancellationToken);
        sesion.Messages.Add(new ChatMessage { Origen = "user", Texto = texto });
        sesion.Messages.Add(new ChatMessage { Origen = "bot", Texto = respuesta });
        await _context.SaveChangesAsync(cancellationToken);
        return new ChatResponseDto { SessionId = sessionId, Respuesta = respuesta };
    }

    private async Task<string> CrearRespuestaAsync(string mensaje, CancellationToken cancellationToken)
    {
        var texto = mensaje.ToLowerInvariant();
        if (texto.Contains("hola") || texto.Contains("buenas")) return "¡Hola! Soy Nogalito. Te ayudo con muebles, entregas, pagos y nuestro taller.";
        if (texto.Contains("envío") || texto.Contains("envio") || texto.Contains("entrega")) return "Entregamos en Bogotá y coordinamos despachos nacionales. El envío es gratis en compras desde $ 500.000.";
        if (texto.Contains("garant")) return "Todos nuestros muebles tienen 10 años de garantía de fábrica por estructura y fabricación.";
        if (texto.Contains("armado")) return "Puedes solicitar armado opcional por $ 89.000 al momento de coordinar tu entrega.";
        if (texto.Contains("ubic") || texto.Contains("taller") || texto.Contains("dirección") || texto.Contains("direccion")) return "Nuestro taller está en Cra. 56 #17-40, Puente Aranda, Bogotá. Atendemos de lunes a viernes de 8:00 a 17:00 y sábados de 8:00 a 12:00.";
        if (texto.Contains("pago") || texto.Contains("cuota") || texto.Contains("pse") || texto.Contains("addi")) return "Puedes pagar con tarjeta, PSE, Addi o Sistecrédito. También podemos orientarte sobre cuotas.";
        if (texto.Contains("devol") || texto.Contains("cambio")) return "Tienes 30 días para pensarlo. Escríbenos con tu pedido y te acompañamos en el proceso.";
        if (texto.Contains("medida") || texto.Contains("personal")) return "Trabajamos directamente desde el taller. Cuéntanos qué medida necesitas por WhatsApp y revisamos la viabilidad.";

        var producto = await _context.Products.AsNoTracking().Where(p => p.Activo)
            .FirstOrDefaultAsync(p => texto.Contains(p.Nombre.ToLower()), cancellationToken);
        if (producto is not null) return $"{producto.Nombre} está en nuestro catálogo por $ {producto.PrecioCOP:N0} COP. Puedes verlo en detalle desde el catálogo.";
        return "Puedo ayudarte con envíos, pagos, garantía, armado, medidas o el taller. Si prefieres, escríbenos al WhatsApp 300 000 0000.";
    }
}
