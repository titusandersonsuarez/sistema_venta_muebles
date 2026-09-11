declare namespace JSX {
  interface IntrinsicElements {
    'model-viewer': React.DetailedHTMLProps<React.HTMLAttributes<HTMLElement>, HTMLElement> & { src?: string; 'ios-src'?: string; crossorigin?: string; alt?: string; ar?: boolean; 'ar-modes'?: string; 'ar-scale'?: string; 'ar-placement'?: string; 'camera-controls'?: boolean; 'touch-action'?: string; 'shadow-intensity'?: string; 'environment-image'?: string; 'auto-rotate'?: boolean; loading?: 'auto' | 'lazy' | 'eager' }
  }
}
