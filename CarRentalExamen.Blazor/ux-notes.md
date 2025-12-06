# Velocity Fleet Command – Dashboard & Cars UX Notes

- Design tokens: use `wwwroot/css/design-system.css` for primary/accent, surfaces, text, radius scale, spacing, shadows, motion (`--vf-*` and legacy `--velocity-*` aliases). New layout styles live in `wwwroot/css/app.css` under “Command Center”.
- Dashboard: hero with operator badge, quick chips, KPI grid, “Today at a glance” mini-cards, revenue and utilization spark bars (derived from available stats), rentals-per-month list, and top cars panel. Skeletons show while loading; empty states when data is missing.
- Cars: toolbar with search, status chips, and sorting; themed table with status pills + quick-change menu; card layout for mobile; right-side drawer for details; delete confirmation modal; toast feedback for CRUD; pagination; operator/permission copy around actions.
- Components share rounded cards, pill badges, and hover/focus treatments matching login/sidebar branding for a cohesive SaaS control-center feel.
- Details pages: shared sticky hero with status pill, meta chips, primary CTA + actions dropdown; 3-column grid (summary cards, tabbed main, actions/audit) that stacks on mobile; consistent status pills/tabs/card-compact pattern; destructive actions gated by modals; skeletons/empty states included.
