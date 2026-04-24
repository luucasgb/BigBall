// BigBall — shared UI primitives for all screens.
// Phone frame + status bar + home bar, icons, flag placeholder, avatar stack,
// bottom nav, common rows. Kept presentational — all data is passed in.

const { useState, useMemo } = React;

/* ─── Icons (stroke-based line set, 20px default) ─────────── */
const Icon = ({ d, size = 20, fill, stroke = 'currentColor', strokeWidth = 1.6 }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill={fill || 'none'}
    stroke={stroke} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round">
    {d}
  </svg>
);

const BBIcons = {
  home:      <Icon d={<path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z"/>} />,
  trophy:    <Icon d={<><path d="M8 4h8v4a4 4 0 0 1-8 0z"/><path d="M6 4H4v2a3 3 0 0 0 3 3"/><path d="M18 4h2v2a3 3 0 0 1-3 3"/><path d="M9 14h6l-1 4h-4z"/><path d="M8 20h8"/></>} />,
  calendar:  <Icon d={<><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 9h18M8 3v4M16 3v4"/></>} />,
  bell:      <Icon d={<><path d="M6 15V11a6 6 0 1 1 12 0v4l1.5 2H4.5z"/><path d="M10 20a2 2 0 0 0 4 0"/></>} />,
  user:      <Icon d={<><circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/></>} />,
  plus:      <Icon d={<path d="M12 5v14M5 12h14"/>} />,
  search:    <Icon d={<><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></>} />,
  chevron:   <Icon d={<path d="m9 6 6 6-6 6"/>} />,
  chevronL:  <Icon d={<path d="m15 6-6 6 6 6"/>} />,
  chevronD:  <Icon d={<path d="m6 9 6 6 6-6"/>} />,
  close:     <Icon d={<path d="M6 6l12 12M18 6 6 18"/>} />,
  check:     <Icon d={<path d="m5 12 5 5L20 7"/>} />,
  dots:      <Icon d={<><circle cx="5" cy="12" r="1.3" fill="currentColor"/><circle cx="12" cy="12" r="1.3" fill="currentColor"/><circle cx="19" cy="12" r="1.3" fill="currentColor"/></>} size={20}/>,
  users:     <Icon d={<><circle cx="9" cy="8" r="3.5"/><path d="M3 20a6 6 0 0 1 12 0"/><circle cx="17" cy="9" r="2.5"/><path d="M15 20a5 5 0 0 1 6-4"/></>} />,
  lock:      <Icon d={<><rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/></>} />,
  unlock:    <Icon d={<><rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 7-1"/></>} />,
  globe:     <Icon d={<><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/></>} />,
  share:     <Icon d={<><path d="M16 6l-4-3-4 3"/><path d="M12 3v12"/><path d="M5 12v7a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-7"/></>} />,
  copy:      <Icon d={<><rect x="8" y="8" width="12" height="12" rx="2"/><path d="M16 8V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h2"/></>} />,
  whistle:   <Icon d={<><path d="M15 9a6 6 0 1 1-6 6"/><circle cx="15" cy="9" r="0.8" fill="currentColor"/><path d="M15 9l5-3"/></>} />,
  ball:      <Icon d={<><circle cx="12" cy="12" r="9"/><path d="M12 3v4M12 17v4M3 12h4M17 12h4M7 7l2.5 2.5M14.5 14.5 17 17M17 7l-2.5 2.5M9.5 14.5 7 17"/></>} />,
  pen:       <Icon d={<path d="m4 20 3-1 11-11-2-2L5 17zM14 6l2 2"/>} />,
  alert:     <Icon d={<><path d="M12 3 2 20h20z"/><path d="M12 10v5M12 18v.01"/></>} />,
  filter:    <Icon d={<path d="M4 5h16l-6 8v6l-4-2v-4z"/>} />,
  sparkle:   <Icon d={<path d="M12 3v4m0 10v4M3 12h4m10 0h4M6 6l2.5 2.5m7 7L18 18M18 6l-2.5 2.5m-7 7L6 18"/>} />,
};

/* ─── Phone frame ─────────────────────────────────────────── */
function Phone({ theme = 'dark', children }) {
  return (
    <div className={`bb-phone theme-${theme}`}>
      <StatusBar theme={theme} />
      {children}
      <div className="bb-homebar" />
    </div>
  );
}

function StatusBar({ theme, time = '14:06' }) {
  return (
    <div className="bb-status">
      <span>{time}</span>
      <div className="bb-status-right">
        {/* signal */}
        <svg width="17" height="11" viewBox="0 0 17 11" fill="currentColor">
          <rect x="0" y="7" width="3" height="4" rx="0.5"/>
          <rect x="4.5" y="5" width="3" height="6" rx="0.5"/>
          <rect x="9" y="2.5" width="3" height="8.5" rx="0.5"/>
          <rect x="13.5" y="0" width="3" height="11" rx="0.5"/>
        </svg>
        {/* wifi */}
        <svg width="16" height="11" viewBox="0 0 16 11" fill="currentColor">
          <path d="M8 10.5a1.2 1.2 0 1 0 0-2.4 1.2 1.2 0 0 0 0 2.4zM3.4 5.7a7 7 0 0 1 9.2 0l-1.3 1.4a5 5 0 0 0-6.6 0zM.6 2.9a11 11 0 0 1 14.8 0l-1.3 1.4a9 9 0 0 0-12.2 0z"/>
        </svg>
        {/* battery */}
        <svg width="26" height="11" viewBox="0 0 26 11" fill="none" stroke="currentColor">
          <rect x="0.5" y="0.5" width="22" height="10" rx="2.5"/>
          <rect x="2" y="2" width="18" height="7" rx="1" fill="currentColor"/>
          <rect x="23.5" y="3.5" width="1.5" height="4" rx="0.5" fill="currentColor" stroke="none"/>
        </svg>
      </div>
    </div>
  );
}

/* ─── Flag / country placeholder (2-letter code, two-tone) ── */
const FLAG_PALETTE = {
  BRA: ['#009739', '#FEDF00'],
  ARG: ['#75AADB', '#ffffff'],
  FRA: ['#0055A4', '#EF4135'],
  GER: ['#000000', '#DD0000'],
  ESP: ['#AA151B', '#F1BF00'],
  ENG: ['#ffffff', '#CE1124'],
  POR: ['#006600', '#FF0000'],
  NED: ['#AE1C28', '#21468B'],
  MEX: ['#006847', '#CE1126'],
  USA: ['#3C3B6E', '#B22234'],
  CAN: ['#D80621', '#ffffff'],
  URU: ['#7B9FD1', '#ffffff'],
  CRO: ['#171796', '#ff0000'],
  ITA: ['#008C45', '#CD212A'],
  JPN: ['#BC002D', '#ffffff'],
  KOR: ['#CD2E3A', '#0047A0'],
  BEL: ['#000000', '#FAE042'],
  SUI: ['#DA291C', '#ffffff'],
  MAR: ['#C1272D', '#006233'],
  SEN: ['#00853F', '#FDEF42'],
  AUS: ['#00843D', '#FFCD00'],
};

function Flag({ code, size = 28, round = true }) {
  const [a, b] = FLAG_PALETTE[code] || ['#4a5a6a', '#2a3542'];
  const radius = round ? '50%' : size * 0.18;
  return (
    <div style={{
      width: size, height: size, borderRadius: radius,
      background: `linear-gradient(135deg, ${a} 50%, ${b} 50%)`,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      flexShrink: 0, overflow: 'hidden',
      boxShadow: 'inset 0 0 0 1px rgba(0,0,0,0.12)',
      fontSize: size * 0.32,
      fontWeight: 700,
      color: 'rgba(255,255,255,0.95)',
      textShadow: '0 1px 1px rgba(0,0,0,0.35)',
      letterSpacing: '-0.02em',
      fontFamily: 'var(--font-display)',
    }}>
      {code}
    </div>
  );
}

/* ─── Avatar ──────────────────────────────────────────────── */
function Avatar({ name, size = 28, color }) {
  const initials = name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  // Deterministic hue from name
  const h = useMemo(() => {
    let x = 0; for (const c of name) x = (x * 31 + c.charCodeAt(0)) % 360; return x;
  }, [name]);
  const bg = color || `oklch(0.55 0.08 ${h})`;
  return (
    <div style={{
      width: size, height: size, borderRadius: '50%',
      background: bg, color: '#fff',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontSize: size * 0.38, fontWeight: 600, letterSpacing: '-0.02em',
      flexShrink: 0,
    }}>{initials}</div>
  );
}

function AvatarStack({ names, size = 22, max = 4 }) {
  const shown = names.slice(0, max);
  const rest = names.length - shown.length;
  return (
    <div style={{ display: 'flex', alignItems: 'center' }}>
      {shown.map((n, i) => (
        <div key={i} style={{ marginLeft: i === 0 ? 0 : -8, border: '2px solid var(--bg-elev)', borderRadius: '50%' }}>
          <Avatar name={n} size={size} />
        </div>
      ))}
      {rest > 0 && (
        <div style={{
          marginLeft: -8,
          width: size, height: size, borderRadius: '50%',
          background: 'var(--chip-bg)', color: 'var(--fg-2)',
          border: '2px solid var(--bg-elev)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: size * 0.36, fontWeight: 600,
        }}>+{rest}</div>
      )}
    </div>
  );
}

/* ─── Bottom tab bar ──────────────────────────────────────── */
function BottomNav({ active = 'home' }) {
  const tabs = [
    { id: 'home', icon: BBIcons.trophy, label: 'Bolões' },
    { id: 'calendar', icon: BBIcons.calendar, label: 'Calendário' },
    { id: 'discover', icon: BBIcons.globe, label: 'Descobrir' },
    { id: 'profile', icon: BBIcons.user, label: 'Perfil' },
  ];
  return (
    <div className="bb-bottomnav">
      {tabs.map(t => (
        <div key={t.id} className={'bb-tab' + (t.id === active ? ' active' : '')}>
          {t.icon}
          <span>{t.label}</span>
        </div>
      ))}
    </div>
  );
}

/* ─── Score box (tabular numeric) ─────────────────────────── */
function ScoreBox({ value, editable, size = 'md', tone = 'default' }) {
  const h = size === 'lg' ? 64 : size === 'sm' ? 32 : 44;
  const fs = size === 'lg' ? 32 : size === 'sm' ? 17 : 24;
  const border = tone === 'correct' ? 'var(--success)'
    : tone === 'wrong' ? 'var(--danger)'
    : editable ? 'var(--line-strong)' : 'var(--line)';
  return (
    <div style={{
      minWidth: h, height: h,
      padding: '0 10px',
      borderRadius: 10,
      border: `1.5px solid ${border}`,
      background: editable ? 'var(--bg-elev)' : 'transparent',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontFamily: 'var(--font-display)',
      fontSize: fs,
      fontWeight: 600,
      fontVariantNumeric: 'tabular-nums',
      letterSpacing: '-0.02em',
      color: 'var(--fg)',
    }}>{value ?? '—'}</div>
  );
}

/* ─── Divider ─────────────────────────────────────────────── */
const Div = ({ mt = 0, mb = 0 }) => (
  <div style={{ height: 1, background: 'var(--line)', marginTop: mt, marginBottom: mb }} />
);

/* ─── Screen label ────────────────────────────────────────── */
function Label({ children }) {
  return (
    <div style={{
      fontSize: 11, fontWeight: 600, color: 'var(--fg-3)',
      letterSpacing: '0.08em', textTransform: 'uppercase',
      margin: '4px 0 10px',
    }}>{children}</div>
  );
}

/* export to window */
Object.assign(window, {
  BBIcons, Icon, Phone, StatusBar, Flag, Avatar, AvatarStack,
  BottomNav, ScoreBox, Div, Label,
});
