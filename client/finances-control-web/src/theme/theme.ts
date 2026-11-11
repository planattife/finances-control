export const theme = {
  colors: {
    primary: {
      main: '#047857',
      hover: '#059669',
      active: '#065f46',
      light: '#d1fae5',
      dark: '#064e3b',
      contrast: '#ffffff',
    },
    background: {
      app: '#f7fafc',
      page: '#f6f8fb',
      surface: '#ffffff',
      surfaceAlt: '#f8fbfd',
      surfaceMuted: '#eef7f4',
    },
    text: {
      primary: '#0f172a',
      body: '#122033',
      input: '#102033',
      label: '#1f2a3d',
      secondary: '#526174',
      muted: '#94a3b8',
      inverse: '#ffffff',
      disabled: '#94a3b8',
    },
    border: {
      default: '#d6dee8',
      strong: '#adc0d5',
      focus: '#059669',
    },
    state: {
      success: '#047857',
      warning: '#d97706',
      error: '#dc2626',
      info: '#2563eb',
    },
    component: {
      disabledBackground: '#eef7f4',
      disabledText: '#94a3b8',
      hoverBackground: '#eef7f4',
      focusRing: 'rgba(5, 150, 105, 0.28)',
      inputFocusRing: 'rgba(5, 150, 105, 0.14)',
      linkUnderlineHover: 'rgba(6, 95, 70, 0.45)',
    },
    shadow: {
      buttonPrimary: '0 16px 34px rgba(4, 120, 87, 0.22)',
      buttonPrimaryHover: '0 18px 38px rgba(4, 120, 87, 0.28)',
      buttonPrimaryActive: '0 10px 24px rgba(4, 120, 87, 0.2)',
      illustration: '0 28px 50px rgba(2, 6, 23, 0.36)',
    },
    overlay: {
      formAccent: 'rgba(16, 185, 129, 0.08)',
      visualPanelStart: 'rgba(15, 23, 42, 0.94)',
      visualPanelEnd: 'rgba(13, 65, 80, 0.92)',
      visualAccent: 'rgba(16, 185, 129, 0.16)',
      visualHighlight: 'rgba(45, 212, 191, 0.28)',
    },
  },
} as const

export type Theme = typeof theme
