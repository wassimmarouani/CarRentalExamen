# Auth UI Notes

- Tokens: `wwwroot/css/design-system.css` holds the palette (primary #0ea5e9, accent #f97316), surfaces (dark carbon + light card), text/muted, borders, radius scale, spacing scale, shadows, and motion curves with legacy `--vf-*` aliases.
- Components: branded lockup (logo mark + app name + secure badge), authority chip (“Operator Access Enabled”), premium auth card (max ~460px), feature bullets, pill badges, password show/hide toggles, inline error/success states, strength meter + requirement list, remember-me row, success toast.
- UX: login validation runs on field change; remember-me writes tokens to `localStorage`, otherwise to `sessionStorage`; forgot-password link routes to security mailto; register blocks submit on confirm mismatch, shows inline requirements + strength, then displays a success toast before redirecting to the dashboard.
- Responsive: desktop split layout with centered card; tablet/mobile stack with the brand hero first; consistent spacing via tokenized gaps and rounded cards; focus, hover, and tap targets keep ≥44px sizing.
