// BigBall — screen compositions. Each Screen* returns a <Phone>-wrapped mobile screen.
// All content is placeholder/sample representative of PRD flows.

const { Phone, BBIcons, Flag, Avatar, AvatarStack, BottomNav, ScoreBox, Label, Div, Icon } = window;

/* ════════════════════════════════════════════════════════════
   1) LOGIN — e-mail/senha + Google OAuth (PRD 4.1)
   ════════════════════════════════════════════════════════════ */
function ScreenLogin({ theme = 'dark' }) {
  return (
    <Phone theme={theme}>
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', padding: '40px 28px 24px' }}>
        {/* wordmark */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginTop: 60 }}>
          <div style={{
            width: 44, height: 44, borderRadius: 12,
            background: 'var(--brand-strong)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--brand-ink)',
          }}>
            <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
              <circle cx="12" cy="12" r="10" fill="none" stroke="currentColor" strokeWidth="1.6"/>
              <path d="M12 4l2.5 4.5L20 10l-4 4 1 5.5L12 17l-5 2.5L8 14l-4-4 5.5-1.5z" fill="currentColor" opacity=".9"/>
            </svg>
          </div>
          <div style={{
            fontFamily: 'var(--font-display)',
            fontSize: 30, fontWeight: 700, letterSpacing: '-0.03em',
          }}>BigBall</div>
        </div>

        <div style={{ marginTop: 18, marginBottom: 36 }}>
          <div style={{ fontSize: 15, color: 'var(--fg-2)', maxWidth: 260, lineHeight: 1.45 }}>
            Bolões da Copa do Mundo 2026.
            Palpite, acompanhe e dispute o topo do ranking.
          </div>
        </div>

        <input className="bb-input" placeholder="E-mail" defaultValue="joao.pereira@gmail.com" />
        <div style={{ height: 10 }}/>
        <input className="bb-input" placeholder="Senha" type="password" defaultValue="••••••••••" />
        <div style={{ textAlign: 'right', margin: '10px 2px 20px', fontSize: 13, color: 'var(--fg-2)' }}>
          Esqueci minha senha
        </div>

        <button className="bb-btn bb-btn-primary">Entrar</button>

        <div style={{ display: 'flex', alignItems: 'center', gap: 10, margin: '22px 0' }}>
          <div style={{ flex: 1, height: 1, background: 'var(--line)' }}/>
          <div style={{ fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.1em' }}>OU</div>
          <div style={{ flex: 1, height: 1, background: 'var(--line)' }}/>
        </div>

        <button className="bb-btn bb-btn-ghost">
          <svg width="18" height="18" viewBox="0 0 48 48">
            <path fill="#EA4335" d="M24 9.5c3.54 0 6.7 1.22 9.2 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
            <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
            <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
            <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
          </svg>
          Continuar com Google
        </button>

        <div style={{ flex: 1 }}/>
        <div style={{ textAlign: 'center', fontSize: 13, color: 'var(--fg-2)' }}>
          Novo por aqui? <span style={{ color: 'var(--fg)', fontWeight: 600 }}>Criar conta</span>
        </div>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   2) HOME — Meus Bolões (PRD 4.3, 4.10)
   ════════════════════════════════════════════════════════════ */
function ScreenHome({ theme = 'dark' }) {
  const pools = [
    { name: 'Família Silva 2026', members: 12, rank: 3, pts: 184, leader: 'Ana L.', leaderPts: 212, you: 184, next: 'BRA × SUI', nextIn: '2h 14m' },
    { name: 'Trampo TechCo', members: 28, rank: 1, pts: 267, leader: 'Você', leaderPts: 267, you: 267, next: 'ARG × MEX', nextIn: 'amanhã' },
    { name: 'Galera do FC', members: 8, rank: 5, pts: 98, leader: 'Rafa M.', leaderPts: 141, you: 98, next: 'GER × JPN', nextIn: '3d 4h' },
  ];
  return (
    <Phone theme={theme}>
      <div className="bb-header" style={{ paddingTop: 10 }}>
        <div>
          <div style={{ fontSize: 12, color: 'var(--fg-3)', letterSpacing: '0.04em' }}>Olá,</div>
          <div className="bb-header-title">João</div>
        </div>
        <div style={{ flex: 1 }}/>
        <div className="bb-iconbtn">{BBIcons.bell}</div>
        <div className="bb-iconbtn">{BBIcons.plus}</div>
      </div>

      <div className="bb-scroll">
        {/* featured banner: next match for user's best pool */}
        <div className="bb-card" style={{ padding: 0, overflow: 'hidden', marginBottom: 20 }}>
          <div style={{ padding: '14px 16px 4px', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6 }}>
            <span className="bb-chip">Próximo · Trampo TechCo</span>
          </div>
          <div style={{ padding: '10px 18px 14px', display: 'flex', alignItems: 'center', gap: 16 }}>
            <div style={{ flex: 1, textAlign: 'center', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10 }}>
              <Flag code="ARG" size={42} />
              <div style={{ fontSize: 13, fontWeight: 600, lineHeight: 1 }}>Argentina</div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.06em' }}>SEX 14 · 16:00</div>
              <div style={{
                fontFamily: 'var(--font-display)', fontSize: 22, fontWeight: 600,
                color: 'var(--fg-2)', margin: '2px 0',
              }}>vs</div>
              <div style={{ fontSize: 10, color: 'var(--fg-3)' }}>GRUPO A · J2</div>
            </div>
            <div style={{ flex: 1, textAlign: 'center', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10 }}>
              <Flag code="MEX" size={42} />
              <div style={{ fontSize: 13, fontWeight: 600, lineHeight: 1 }}>México</div>
            </div>
          </div>
          <div style={{
            display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
            padding: '0 16px 14px', fontSize: 11, color: 'var(--fg-2)',
          }}>
            <span style={{ color: 'var(--fg-3)' }}>{BBIcons.lock}</span>
            Palpite fecha em <b style={{ color: 'var(--fg)', fontVariantNumeric: 'tabular-nums' }}>1h 47m</b>
          </div>
          <div style={{
            display: 'flex', alignItems: 'center', gap: 10,
            padding: '12px 16px', background: 'var(--bg-elev-2)',
            borderTop: '1px solid var(--line)',
          }}>
            <span style={{ fontSize: 12, color: 'var(--fg-2)' }}>Seu palpite</span>
            <div style={{ flex: 1 }}/>
            <ScoreBox value="2" size="sm" />
            <span style={{ color: 'var(--fg-3)' }}>×</span>
            <ScoreBox value="1" size="sm" />
            <span style={{ marginLeft: 4, color: 'var(--fg-3)' }}>{BBIcons.pen}</span>
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 10 }}>
          <div style={{ fontSize: 14, fontWeight: 600 }}>Meus bolões</div>
          <div style={{ fontSize: 12, color: 'var(--fg-2)' }}>{pools.length} ativos</div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {pools.map((p, i) => (
            <div key={i} className="bb-card" style={{ padding: 14 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 38, height: 38, borderRadius: 10,
                  background: p.rank === 1 ? 'color-mix(in oklab, var(--brand-strong) 20%, transparent)' : 'var(--chip-bg)',
                  color: p.rank === 1 ? 'var(--brand-strong)' : 'var(--fg-2)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                }}>{BBIcons.trophy}</div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontSize: 14, fontWeight: 600, letterSpacing: '-0.01em' }}>{p.name}</div>
                  <div style={{ fontSize: 12, color: 'var(--fg-2)', marginTop: 1 }}>{p.members} participantes</div>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{
                    fontFamily: 'var(--font-display)', fontWeight: 600,
                    fontSize: 20, letterSpacing: '-0.02em', fontVariantNumeric: 'tabular-nums',
                    color: p.rank === 1 ? 'var(--brand-strong)' : 'var(--fg)',
                  }}>#{p.rank}</div>
                  <div style={{ fontSize: 11, color: 'var(--fg-3)', fontVariantNumeric: 'tabular-nums' }}>{p.pts} pts</div>
                </div>
              </div>

              <div style={{
                marginTop: 12, paddingTop: 10, borderTop: '1px solid var(--line)',
                display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 10,
              }}>
                <div style={{ fontSize: 12, color: 'var(--fg-2)' }}>
                  <span style={{ color: 'var(--fg)', fontWeight: 500 }}>{p.next}</span>
                  <span style={{ margin: '0 8px', opacity: 0.5 }}>·</span>
                  {p.nextIn}
                </div>
                <div style={{ color: 'var(--fg-3)' }}>{BBIcons.chevron}</div>
              </div>
            </div>
          ))}

          <div style={{
            marginTop: 4,
            padding: '14px',
            borderRadius: 16,
            border: '1.5px dashed var(--line-strong)',
            display: 'flex', alignItems: 'center', gap: 12,
            color: 'var(--fg-2)',
            fontSize: 14,
          }}>
            <div style={{
              width: 36, height: 36, borderRadius: 10,
              background: 'var(--chip-bg)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
            }}>{BBIcons.plus}</div>
            <div style={{ flex: 1 }}>
              <div style={{ color: 'var(--fg)', fontWeight: 500 }}>Criar ou entrar em bolão</div>
              <div style={{ fontSize: 12 }}>Público, privado ou com código</div>
            </div>
          </div>
        </div>
      </div>

      <BottomNav active="home" />
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   3) POOL DETAIL — Ranking (PRD 4.10)
   ════════════════════════════════════════════════════════════ */
function ScreenPool({ theme = 'dark' }) {
  const members = [
    { name: 'Ana Luz', pts: 212, b20: 4, b16: 6, pen: 2, trend: '+20', me: false },
    { name: 'Você', pts: 184, b20: 3, b16: 5, pen: 1, trend: '+16', me: true },
    { name: 'Bruno R.', pts: 184, b20: 3, b16: 4, pen: 2, trend: '+15', me: false, tieWith: 'Você' },
    { name: 'Carla M.', pts: 167, b20: 2, b16: 7, pen: 0, trend: '+10', me: false },
    { name: 'Diego F.', pts: 152, b20: 2, b16: 5, pen: 1, trend: '+5',  me: false },
    { name: 'Eva T.',   pts: 141, b20: 2, b16: 4, pen: 1, trend: '0',   me: false },
    { name: 'Fábio S.', pts: 128, b20: 1, b16: 6, pen: 0, trend: '+16', me: false },
  ];
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div className="bb-header-title" style={{ fontSize: 18 }}>Família Silva 2026</div>
        <div className="bb-iconbtn">{BBIcons.dots}</div>
      </div>

      <div className="bb-scroll">
        {/* summary strip */}
        <div style={{
          display: 'flex', gap: 8, marginBottom: 14,
        }}>
          <div className="bb-card" style={{ flex: 1, padding: 12 }}>
            <div style={{ fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.05em', textTransform: 'uppercase' }}>Sua posição</div>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 6, marginTop: 4 }}>
              <div style={{ fontFamily: 'var(--font-display)', fontSize: 28, fontWeight: 600, letterSpacing: '-0.02em' }}>#2</div>
              <div style={{ fontSize: 12, color: 'var(--success)' }}>↑ 1</div>
            </div>
            <div style={{ fontSize: 11, color: 'var(--fg-2)', marginTop: 2, fontVariantNumeric: 'tabular-nums' }}>184 pts · 12 jogos</div>
          </div>
          <div className="bb-card" style={{ flex: 1, padding: 12 }}>
            <div style={{ fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.05em', textTransform: 'uppercase' }}>Líder</div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 4 }}>
              <Avatar name="Ana Luz" size={22} />
              <div style={{ fontSize: 14, fontWeight: 600 }}>Ana Luz</div>
            </div>
            <div style={{ fontSize: 11, color: 'var(--fg-2)', marginTop: 4, fontVariantNumeric: 'tabular-nums' }}>212 pts · +28 vantagem</div>
          </div>
        </div>

        {/* segmented tabs */}
        <div style={{
          display: 'flex',
          background: 'var(--chip-bg)',
          border: '1px solid var(--chip-border)',
          borderRadius: 10, padding: 3,
          marginBottom: 14, fontSize: 13, fontWeight: 500,
        }}>
          {['Ranking', 'Jogos', 'Prêmio', 'Membros'].map((t, i) => (
            <div key={t} style={{
              flex: 1, textAlign: 'center', padding: '7px 0',
              borderRadius: 8,
              background: i === 0 ? 'var(--bg-elev)' : 'transparent',
              color: i === 0 ? 'var(--fg)' : 'var(--fg-2)',
              fontWeight: i === 0 ? 600 : 500,
              boxShadow: i === 0 ? '0 1px 2px rgba(0,0,0,0.08)' : 'none',
            }}>{t}</div>
          ))}
        </div>

        {/* ranking list */}
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <div style={{
            display: 'grid', gridTemplateColumns: '24px 1fr auto auto',
            padding: '6px 2px', gap: 10,
            fontSize: 10, color: 'var(--fg-3)',
            letterSpacing: '0.08em', textTransform: 'uppercase',
          }}>
            <div>#</div><div>Membro</div>
            <div style={{ textAlign: 'right', width: 50 }}>Últ.</div>
            <div style={{ textAlign: 'right', width: 48 }}>Pts</div>
          </div>
          {members.map((m, i) => {
            const pos = i + 1;
            const isTie = m.tieWith;
            return (
              <div key={i} style={{
                display: 'grid', gridTemplateColumns: '24px 1fr auto auto',
                gap: 10, padding: '10px 2px',
                alignItems: 'center',
                borderTop: i === 0 ? 'none' : '1px solid var(--line)',
                background: m.me ? 'color-mix(in oklab, var(--brand-strong) 8%, transparent)' : 'transparent',
                marginLeft: m.me ? -12 : 0, marginRight: m.me ? -12 : 0,
                paddingLeft: m.me ? 14 : 2, paddingRight: m.me ? 14 : 2,
                borderRadius: m.me ? 10 : 0,
                borderTopColor: m.me ? 'transparent' : 'var(--line)',
              }}>
                <div style={{
                  fontFamily: 'var(--font-display)',
                  fontVariantNumeric: 'tabular-nums',
                  fontSize: 15, fontWeight: 600,
                  color: pos === 1 ? 'var(--brand-strong)' : 'var(--fg-2)',
                }}>{pos}</div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
                  <Avatar name={m.name} size={30} />
                  <div style={{ minWidth: 0 }}>
                    <div style={{ fontSize: 14, fontWeight: m.me ? 600 : 500, display: 'flex', alignItems: 'center', gap: 6 }}>
                      {m.name}
                      {m.me && <span style={{ fontSize: 10, color: 'var(--fg-3)', fontWeight: 500 }}>VOCÊ</span>}
                      {isTie && <span style={{ fontSize: 10, color: 'var(--warning)', fontWeight: 500 }} title="Empate">= empate</span>}
                    </div>
                    <div style={{ fontSize: 11, color: 'var(--fg-3)', fontVariantNumeric: 'tabular-nums' }}>
                      {m.b20}× acertos · {m.pen}× pênaltis
                    </div>
                  </div>
                </div>
                <div style={{
                  fontSize: 12, fontVariantNumeric: 'tabular-nums',
                  color: m.trend === '0' ? 'var(--fg-3)' : 'var(--success)',
                  textAlign: 'right', fontWeight: 500,
                }}>{m.trend !== '0' ? '+' + m.trend.replace('+', '') : '—'}</div>
                <div style={{
                  fontFamily: 'var(--font-display)',
                  fontVariantNumeric: 'tabular-nums',
                  fontSize: 17, fontWeight: 600,
                  letterSpacing: '-0.02em',
                  textAlign: 'right',
                }}>{m.pts}</div>
              </div>
            );
          })}
        </div>

        <div style={{
          marginTop: 14, padding: 12,
          background: 'color-mix(in oklab, var(--warning) 10%, transparent)',
          border: '1px solid color-mix(in oklab, var(--warning) 30%, transparent)',
          borderRadius: 12, fontSize: 12, color: 'var(--fg-2)',
          display: 'flex', gap: 10,
        }}>
          <div style={{ color: 'var(--warning)', marginTop: 1 }}>{BBIcons.alert}</div>
          <div>
            <div style={{ color: 'var(--fg)', fontWeight: 500, marginBottom: 2 }}>Empate no ranking</div>
            Você e Bruno R. estão com 184 pts. Desempate ativo por contagem de acertos exatos (20 pts).
          </div>
        </div>
      </div>

      <BottomNav active="home" />
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   4) PREDICTION — Fase de grupos (PRD 4.7)
   ════════════════════════════════════════════════════════════ */
function ScreenPredictGroup({ theme = 'dark' }) {
  const [home, setHome] = React.useState(2);
  const [away, setAway] = React.useState(1);
  const stepper = (val, set) => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 6, alignItems: 'center' }}>
      <button onClick={() => set(v => Math.min(9, v + 1))} style={{
        width: 36, height: 28, borderRadius: 8, border: '1px solid var(--line-strong)',
        background: 'var(--bg-elev)', color: 'var(--fg-2)', cursor: 'pointer',
      }}>+</button>
      <ScoreBox value={val} editable size="lg" />
      <button onClick={() => set(v => Math.max(0, v - 1))} style={{
        width: 36, height: 28, borderRadius: 8, border: '1px solid var(--line-strong)',
        background: 'var(--bg-elev)', color: 'var(--fg-2)', cursor: 'pointer',
      }}>−</button>
    </div>
  );

  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div className="bb-header-title" style={{ fontSize: 16 }}>Palpite · Grupo A</div>
        <div className="bb-iconbtn">{BBIcons.share}</div>
      </div>

      <div className="bb-scroll">
        {/* match card */}
        <div className="bb-card" style={{ padding: 20, textAlign: 'center' }}>
          <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginBottom: 14 }}>
            <span className="bb-chip">Sex 14 jun · 16:00</span>
            <span className="bb-chip">Estádio Azteca</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 18, marginBottom: 18 }}>
            <div style={{ textAlign: 'center', width: 90, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
              <Flag code="ARG" size={56} />
              <div style={{ fontSize: 14, fontWeight: 600, lineHeight: 1 }}>Argentina</div>
            </div>
            <div style={{
              fontFamily: 'var(--font-display)',
              fontSize: 20, color: 'var(--fg-3)', fontWeight: 500,
            }}>vs</div>
            <div style={{ textAlign: 'center', width: 90, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
              <Flag code="MEX" size={56} />
              <div style={{ fontSize: 14, fontWeight: 600, lineHeight: 1 }}>México</div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 18 }}>
            {stepper(home, setHome)}
            <div style={{ fontSize: 24, color: 'var(--fg-3)' }}>×</div>
            {stepper(away, setAway)}
          </div>

          <div style={{
            marginTop: 18, fontSize: 12, color: 'var(--fg-2)',
            display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
          }}>
            {BBIcons.lock}
            Fecha 15:55 · em <b style={{ color: 'var(--fg)', marginLeft: 2 }}>1h 47m</b>
          </div>
        </div>

        <Label>Como pontua este palpite</Label>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {[
            { pts: 20, desc: 'Placar exato', match: home === 2 && away === 1 },
            { pts: 16, desc: 'Vencedor + saldo de gols', match: false },
            { pts: 15, desc: 'Vencedor + 1 placar individual' },
            { pts: 10, desc: 'Só o vencedor' },
            { pts: 5,  desc: 'Só 1 placar individual' },
          ].map(r => (
            <div key={r.pts} style={{
              display: 'flex', alignItems: 'center', gap: 10,
              padding: '8px 12px',
              background: r.match ? 'color-mix(in oklab, var(--success) 14%, transparent)' : 'transparent',
              border: '1px solid ' + (r.match ? 'color-mix(in oklab, var(--success) 40%, transparent)' : 'var(--line)'),
              borderRadius: 10,
            }}>
              <div style={{
                fontFamily: 'var(--font-display)',
                fontVariantNumeric: 'tabular-nums',
                fontSize: 17, fontWeight: 700, width: 28,
                color: r.match ? 'var(--success)' : 'var(--fg-2)',
              }}>{r.pts}</div>
              <div style={{ fontSize: 13, color: 'var(--fg)' }}>{r.desc}</div>
              {r.match && <div style={{ marginLeft: 'auto', fontSize: 11, color: 'var(--success)', fontWeight: 600 }}>ESTE</div>}
            </div>
          ))}
        </div>

        <div style={{ marginTop: 18, display: 'flex', gap: 10 }}>
          <button className="bb-btn bb-btn-secondary" style={{ flex: 1 }}>Cancelar</button>
          <button className="bb-btn bb-btn-primary" style={{ flex: 2 }}>Salvar palpite</button>
        </div>

        <div style={{
          marginTop: 14, fontSize: 11, color: 'var(--fg-3)',
          textAlign: 'center', lineHeight: 1.6,
        }}>
          Pontuação é calculada sobre o placar ao fim do tempo regulamentar (90 min + acréscimos).
        </div>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   5) PREDICTION — Mata-mata (PRD 4.7) com pênaltis obrigatório
   ════════════════════════════════════════════════════════════ */
function ScreenPredictKO({ theme = 'dark' }) {
  const [pk, setPk] = React.useState('BRA');
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div className="bb-header-title" style={{ fontSize: 16 }}>Palpite · Oitavas</div>
        <div className="bb-iconbtn">{BBIcons.share}</div>
      </div>

      <div className="bb-scroll">
        <div className="bb-card" style={{ padding: 20, textAlign: 'center' }}>
          <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginBottom: 14 }}>
            <span className="bb-chip gold">Mata-mata</span>
            <span className="bb-chip">Sáb 29 jun · 13:00</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 16, marginBottom: 16 }}>
            <div style={{ textAlign: 'center', width: 84, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
              <Flag code="BRA" size={56} />
              <div style={{ fontSize: 14, fontWeight: 600, lineHeight: 1 }}>Brasil</div>
            </div>
            <div style={{ fontFamily: 'var(--font-display)', fontSize: 18, color: 'var(--fg-3)' }}>vs</div>
            <div style={{ textAlign: 'center', width: 84, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
              <Flag code="URU" size={56} />
              <div style={{ fontSize: 14, fontWeight: 600, lineHeight: 1 }}>Uruguai</div>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 18 }}>
            <ScoreBox value="1" editable size="lg" />
            <div style={{ fontSize: 22, color: 'var(--fg-3)' }}>×</div>
            <ScoreBox value="1" editable size="lg" />
          </div>
          <div style={{ fontSize: 11, color: 'var(--fg-3)', marginTop: 8 }}>
            Placar ao fim dos 90 min + acréscimos
          </div>
        </div>

        {/* Penalty winner — required in KO */}
        <div className="bb-card" style={{ padding: 16, marginTop: 12, border: '1.5px solid color-mix(in oklab, var(--warning) 40%, transparent)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <div style={{ color: 'var(--warning)' }}>{BBIcons.whistle}</div>
            <div style={{ fontSize: 13, fontWeight: 600 }}>Vencedor nos pênaltis</div>
            <span style={{
              marginLeft: 'auto', fontSize: 10, fontWeight: 600,
              color: 'var(--warning)', letterSpacing: '0.05em',
            }}>OBRIGATÓRIO</span>
          </div>
          <div style={{ fontSize: 12, color: 'var(--fg-2)', marginBottom: 12 }}>
            Se houver disputa de pênaltis, quem você acha que vence? <span style={{ color: 'var(--fg)', fontWeight: 500 }}>+3 pontos bônus</span> se acertar.
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
            {[{ c: 'BRA', n: 'Brasil' }, { c: 'URU', n: 'Uruguai' }].map(t => (
              <button key={t.c} onClick={() => setPk(t.c)} style={{
                padding: 12,
                background: pk === t.c ? 'color-mix(in oklab, var(--brand-strong) 18%, transparent)' : 'var(--bg-elev-2)',
                border: '1.5px solid ' + (pk === t.c ? 'var(--brand-strong)' : 'var(--line)'),
                borderRadius: 12,
                display: 'flex', alignItems: 'center', gap: 10,
                cursor: 'pointer', font: 'inherit', color: 'inherit',
              }}>
                <Flag code={t.c} size={28} />
                <div style={{ flex: 1, textAlign: 'left', fontSize: 14, fontWeight: pk === t.c ? 600 : 500 }}>{t.n}</div>
                {pk === t.c && <div style={{ color: 'var(--brand-strong)' }}>{BBIcons.check}</div>}
              </button>
            ))}
          </div>
        </div>

        <Label>Seus pontos potenciais</Label>
        <div className="bb-card" style={{ padding: 14 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13, padding: '4px 0' }}>
            <div style={{ color: 'var(--fg-2)' }}>Placar exato (1×1)</div>
            <div style={{ fontWeight: 600, fontVariantNumeric: 'tabular-nums' }}>20</div>
          </div>
          <Div mt={4} mb={4} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13, padding: '4px 0' }}>
            <div style={{ color: 'var(--fg-2)' }}>Bônus pênaltis · Brasil vence</div>
            <div style={{ fontWeight: 600, color: 'var(--brand-strong)', fontVariantNumeric: 'tabular-nums' }}>+3</div>
          </div>
          <Div mt={4} mb={4} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 14, padding: '4px 0' }}>
            <div style={{ fontWeight: 600 }}>Máximo possível</div>
            <div style={{
              fontFamily: 'var(--font-display)', fontWeight: 700, fontSize: 18,
              fontVariantNumeric: 'tabular-nums',
              color: 'var(--brand-strong)',
            }}>23 pts</div>
          </div>
        </div>

        <button className="bb-btn bb-btn-primary" style={{ width: '100%', marginTop: 18 }}>
          Salvar palpite
        </button>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   6) CALENDAR — PRD 4.5
   ════════════════════════════════════════════════════════════ */
function ScreenCalendar({ theme = 'dark' }) {
  const days = [
    { d: 13, wd: 'QUI' },
    { d: 14, wd: 'SEX', active: true },
    { d: 15, wd: 'SÁB' },
    { d: 16, wd: 'DOM' },
    { d: 17, wd: 'SEG' },
    { d: 18, wd: 'TER' },
    { d: 19, wd: 'QUA' },
  ];
  const matches = [
    { time: '13:00', home: 'FRA', away: 'SUI', hn: 'França', an: 'Suíça', phase: 'Grupo D · J2', status: 'pred', pred: [2, 1], stadium: 'BMO Field' },
    { time: '16:00', home: 'ARG', away: 'MEX', hn: 'Argentina', an: 'México', phase: 'Grupo A · J2', status: 'open', stadium: 'Azteca', closesIn: '1h 47m' },
    { time: '19:00', home: 'BRA', away: 'SUI', hn: 'Brasil', an: 'Suíça', phase: 'Grupo G · J2', status: 'live', live: [1, 0], minute: '38\'', stadium: 'MetLife' },
    { time: '22:00', home: 'ENG', away: 'USA', hn: 'Inglaterra', an: 'EUA', phase: 'Grupo B · J2', status: 'open', stadium: 'Lumen Field', closesIn: '7h 55m' },
  ];
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-header-title">Calendário</div>
        <div className="bb-iconbtn">{BBIcons.filter}</div>
        <div className="bb-iconbtn">{BBIcons.search}</div>
      </div>

      <div style={{ padding: '0 20px 14px' }}>
        <div style={{
          display: 'flex', alignItems: 'center', gap: 6,
          fontSize: 13, color: 'var(--fg-2)',
          marginBottom: 10,
        }}>
          <span>JUNHO 2026</span>
          <div style={{ color: 'var(--fg-3)' }}>{BBIcons.chevronD}</div>
          <div style={{ flex: 1 }}/>
          <span style={{ fontSize: 11, color: 'var(--fg-3)' }}>Fase de grupos · Rodada 2</span>
        </div>
        <div style={{ display: 'flex', gap: 6, overflow: 'hidden' }}>
          {days.map(day => (
            <div key={day.d} style={{
              flex: 1, padding: '10px 0', textAlign: 'center',
              borderRadius: 10,
              background: day.active ? 'var(--brand-strong)' : 'var(--chip-bg)',
              border: '1px solid ' + (day.active ? 'var(--brand-strong)' : 'var(--chip-border)'),
              color: day.active ? 'var(--brand-ink)' : 'var(--fg-2)',
            }}>
              <div style={{ fontSize: 10, letterSpacing: '0.06em', fontWeight: 500 }}>{day.wd}</div>
              <div style={{
                fontFamily: 'var(--font-display)', fontSize: 18, fontWeight: 600,
                marginTop: 2, letterSpacing: '-0.02em',
              }}>{day.d}</div>
            </div>
          ))}
        </div>
      </div>

      <div className="bb-scroll" style={{ paddingTop: 0 }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {matches.map((m, i) => (
            <div key={i} className="bb-card" style={{ padding: 14, display: 'flex', alignItems: 'stretch', gap: 14 }}>
              <div style={{
                fontFamily: 'var(--font-display)',
                fontSize: 15, fontWeight: 600,
                color: 'var(--fg-2)',
                width: 44, flexShrink: 0,
                fontVariantNumeric: 'tabular-nums',
              }}>
                {m.time}
                <div style={{
                  fontSize: 10, fontFamily: 'var(--font-sans)',
                  fontWeight: 500, letterSpacing: '0.04em',
                  color: 'var(--fg-3)', marginTop: 2,
                }}>{m.phase.split(' · ')[0]}</div>
              </div>
              <div style={{ width: 1, background: 'var(--line)' }}/>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <Flag code={m.home} size={24} />
                  <div style={{ fontSize: 14, fontWeight: 500, flex: 1 }}>{m.hn}</div>
                  {m.status === 'live' && <ScoreBox value={m.live[0]} size="sm" />}
                  {m.status === 'pred' && <span style={{ fontSize: 13, color: 'var(--fg-2)', fontVariantNumeric: 'tabular-nums' }}>{m.pred[0]}</span>}
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginTop: 6 }}>
                  <Flag code={m.away} size={24} />
                  <div style={{ fontSize: 14, fontWeight: 500, flex: 1 }}>{m.an}</div>
                  {m.status === 'live' && <ScoreBox value={m.live[1]} size="sm" />}
                  {m.status === 'pred' && <span style={{ fontSize: 13, color: 'var(--fg-2)', fontVariantNumeric: 'tabular-nums' }}>{m.pred[1]}</span>}
                </div>
                <div style={{
                  display: 'flex', alignItems: 'center', gap: 6,
                  marginTop: 10,
                  fontSize: 11, color: 'var(--fg-3)',
                }}>
                  {m.status === 'live' && <span className="bb-chip live">Ao vivo {m.minute}</span>}
                  {m.status === 'pred' && <span className="bb-chip gold">Palpite salvo {m.pred[0]}×{m.pred[1]}</span>}
                  {m.status === 'open' && <span className="bb-chip">Fecha em {m.closesIn}</span>}
                  <span style={{ marginLeft: 'auto' }}>{m.stadium}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <BottomNav active="calendar" />
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   7) CREATE POOL — PRD 4.3
   ════════════════════════════════════════════════════════════ */
function ScreenCreatePool({ theme = 'dark' }) {
  const [type, setType] = React.useState('private');
  const [cost, setCost] = React.useState(true);
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.close}</div>
        <div className="bb-header-title" style={{ fontSize: 18 }}>Novo bolão</div>
        <div style={{ fontSize: 13, color: 'var(--fg-3)' }}>1/2</div>
      </div>

      <div className="bb-scroll">
        <Label>Nome</Label>
        <input className="bb-input" defaultValue="Copa do Trampo 2026" />

        <Label>Descrição <span style={{ color: 'var(--fg-3)', textTransform: 'none', fontWeight: 500 }}>(opcional)</span></Label>
        <textarea className="bb-input" style={{ height: 76, padding: '12px 14px', resize: 'none' }}
          defaultValue="Galera do time de engenharia. Entrada R$20 no PIX até o jogo de abertura." />

        <Label>Visibilidade</Label>
        <div style={{ display: 'flex', gap: 8 }}>
          {[
            { id: 'public', icon: BBIcons.globe, title: 'Público', sub: 'Qualquer pessoa pode entrar' },
            { id: 'private', icon: BBIcons.lock, title: 'Privado', sub: 'Só com código de convite' },
          ].map(opt => (
            <button key={opt.id} onClick={() => setType(opt.id)} style={{
              flex: 1, padding: 14, textAlign: 'left',
              background: type === opt.id ? 'color-mix(in oklab, var(--brand-strong) 14%, transparent)' : 'var(--bg-elev)',
              border: '1.5px solid ' + (type === opt.id ? 'var(--brand-strong)' : 'var(--line)'),
              borderRadius: 12,
              color: 'inherit', font: 'inherit',
              display: 'flex', flexDirection: 'column', gap: 8,
              cursor: 'pointer',
            }}>
              <div style={{ color: type === opt.id ? 'var(--brand-strong)' : 'var(--fg-2)' }}>{opt.icon}</div>
              <div style={{ fontSize: 14, fontWeight: 600 }}>{opt.title}</div>
              <div style={{ fontSize: 11, color: 'var(--fg-2)', lineHeight: 1.3 }}>{opt.sub}</div>
            </button>
          ))}
        </div>

        <Label>Premiação</Label>
        <input className="bb-input" defaultValue="1º uma pizza grande · 2º refrigerante" />

        <Label>Custo de entrada <span style={{ color: 'var(--fg-3)', textTransform: 'none', fontWeight: 500 }}>(opcional)</span></Label>
        <div style={{
          display: 'flex', alignItems: 'center', gap: 10,
          background: 'var(--bg-elev)', border: '1px solid var(--line-strong)',
          borderRadius: 12, padding: '10px 14px', height: 48,
        }}>
          <div style={{ fontSize: 14, color: 'var(--fg-2)' }}>Tem custo?</div>
          <div style={{ flex: 1 }}/>
          <button onClick={() => setCost(!cost)} style={{
            width: 44, height: 24, borderRadius: 12,
            background: cost ? 'var(--brand-strong)' : 'var(--chip-bg)',
            border: 'none', padding: 2, cursor: 'pointer', position: 'relative',
          }}>
            <div style={{
              width: 20, height: 20, borderRadius: '50%',
              background: cost ? 'var(--brand-ink)' : 'var(--fg-3)',
              transform: `translateX(${cost ? 20 : 0}px)`, transition: 'transform .15s',
            }}/>
          </button>
        </div>

        {cost && (
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <div style={{
              height: 48, padding: '0 14px', background: 'var(--bg-elev)',
              border: '1px solid var(--line-strong)', borderRadius: 12,
              display: 'flex', alignItems: 'center', color: 'var(--fg-2)',
              fontSize: 14,
            }}>R$</div>
            <input className="bb-input" style={{ flex: 1 }} defaultValue="20,00" />
          </div>
        )}

        <div style={{
          marginTop: 14, padding: 12,
          background: 'var(--chip-bg)', borderRadius: 10,
          fontSize: 11, color: 'var(--fg-2)', lineHeight: 1.5,
        }}>
          O valor é apenas informativo — cobrança e pagamento ficam por conta do organizador, fora do app.
        </div>

        <div style={{ height: 16 }}/>
        <button className="bb-btn bb-btn-primary" style={{ width: '100%' }}>Continuar</button>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   8) JOIN BY CODE — PRD 4.4
   ════════════════════════════════════════════════════════════ */
function ScreenJoinCode({ theme = 'dark' }) {
  const code = ['K', 'P', '3', 'R', '9', 'A'];
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div className="bb-header-title" style={{ fontSize: 17 }}>Entrar com código</div>
      </div>

      <div style={{ flex: 1, padding: '8px 28px 28px', display: 'flex', flexDirection: 'column' }}>
        <div style={{
          width: 72, height: 72, borderRadius: 18,
          background: 'var(--chip-bg)', border: '1px solid var(--chip-border)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          margin: '6px auto 18px', color: 'var(--brand-strong)',
        }}>
          <Icon size={32} d={<><rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/></>} />
        </div>

        <div style={{ fontSize: 20, fontWeight: 600, textAlign: 'center', fontFamily: 'var(--font-display)', letterSpacing: '-0.02em' }}>
          Digite o código do bolão
        </div>
        <div style={{ fontSize: 13, color: 'var(--fg-2)', textAlign: 'center', margin: '6px 8px 24px', lineHeight: 1.5 }}>
          O organizador compartilhou um código de 6 caracteres.
        </div>

        <div style={{ display: 'flex', gap: 8, justifyContent: 'center' }}>
          {code.map((c, i) => (
            <div key={i} style={{
              width: 42, height: 54, borderRadius: 10,
              background: 'var(--bg-elev)',
              border: '1.5px solid ' + (i === 5 ? 'var(--brand-strong)' : 'var(--line-strong)'),
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontFamily: 'var(--font-display)', fontSize: 24, fontWeight: 600,
              letterSpacing: '-0.02em',
            }}>{c}</div>
          ))}
        </div>

        <div style={{
          marginTop: 20, padding: 14,
          background: 'var(--bg-elev)',
          border: '1px solid var(--line)',
          borderRadius: 12,
          display: 'flex', alignItems: 'center', gap: 12,
        }}>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.05em', textTransform: 'uppercase' }}>Bolão encontrado</div>
            <div style={{ fontSize: 14, fontWeight: 600, marginTop: 3 }}>Copa do Trampo 2026</div>
            <div style={{ fontSize: 12, color: 'var(--fg-2)', marginTop: 2 }}>28 participantes · Rafa S.</div>
          </div>
          <AvatarStack names={['Rafa S.', 'Ana P.', 'Bruno', 'Clara', 'Dani']} size={26} max={3} />
        </div>

        <div style={{ flex: 1 }}/>

        <button className="bb-btn bb-btn-primary" style={{ width: '100%' }}>Entrar no bolão</button>
        <div style={{ textAlign: 'center', marginTop: 14, fontSize: 12, color: 'var(--fg-2)' }}>
          Sem código? <span style={{ color: 'var(--fg)', fontWeight: 500 }}>Ver bolões públicos</span>
        </div>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   9) MATCH RESULT — breakdown de pontos (PRD 4.6, 4.8)
   ════════════════════════════════════════════════════════════ */
function ScreenMatchResult({ theme = 'dark' }) {
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div className="bb-header-title" style={{ fontSize: 16 }}>Jogo encerrado</div>
        <div className="bb-iconbtn">{BBIcons.share}</div>
      </div>

      <div className="bb-scroll">
        <div className="bb-card" style={{ padding: 20, textAlign: 'center' }}>
          <div style={{ display: 'flex', gap: 6, justifyContent: 'center', marginBottom: 14 }}>
            <span className="bb-chip">Oitavas · Sáb 29 jun</span>
            <span className="bb-chip gold">Encerrado</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, marginBottom: 10 }}>
            <div style={{ width: 80, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10 }}>
              <Flag code="BRA" size={44} />
              <div style={{ fontSize: 13, color: 'var(--fg-2)', lineHeight: 1 }}>Brasil</div>
            </div>
            <div style={{
              fontFamily: 'var(--font-display)',
              fontSize: 42, fontWeight: 700, letterSpacing: '-0.03em',
              display: 'flex', alignItems: 'center', gap: 10,
              fontVariantNumeric: 'tabular-nums',
            }}>
              <div>1</div>
              <div style={{ fontSize: 28, color: 'var(--fg-3)', fontWeight: 500 }}>×</div>
              <div>1</div>
            </div>
            <div style={{ width: 80, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10 }}>
              <Flag code="URU" size={44} />
              <div style={{ fontSize: 13, color: 'var(--fg-2)', lineHeight: 1 }}>Uruguai</div>
            </div>
          </div>
          <div style={{ fontSize: 11, color: 'var(--fg-3)', marginBottom: 10 }}>
            Tempo regulamentar · Brasil venceu nos pênaltis 4-3
          </div>
          <div style={{
            display: 'inline-flex', alignItems: 'center', gap: 8,
            fontSize: 11, color: 'var(--fg-3)',
            padding: '4px 10px', borderRadius: 999,
            background: 'var(--chip-bg)',
          }}>
            {BBIcons.globe} Resultado: provedor oficial
          </div>
        </div>

        <Label>Seu palpite</Label>
        <div className="bb-card" style={{ padding: 0, overflow: 'hidden' }}>
          <div style={{
            display: 'flex', alignItems: 'center', gap: 14,
            padding: 14,
          }}>
            <div style={{ flex: 1, display: 'flex', alignItems: 'center', gap: 10 }}>
              <Flag code="BRA" size={22} /><span style={{ fontSize: 14 }}>Brasil</span>
            </div>
            <ScoreBox value="1" size="sm" tone="correct" />
            <span style={{ color: 'var(--fg-3)' }}>×</span>
            <ScoreBox value="1" size="sm" tone="correct" />
            <div style={{ flex: 1, display: 'flex', alignItems: 'center', gap: 10, justifyContent: 'flex-end' }}>
              <span style={{ fontSize: 14 }}>Uruguai</span><Flag code="URU" size={22} />
            </div>
          </div>
          <div style={{ padding: '10px 14px', background: 'var(--bg-elev-2)', borderTop: '1px solid var(--line)', fontSize: 12, color: 'var(--fg-2)', display: 'flex', alignItems: 'center', gap: 8 }}>
            {BBIcons.whistle} Vencedor pênaltis: <b style={{ color: 'var(--fg)' }}>Brasil</b>
            <span style={{ marginLeft: 'auto', color: 'var(--success)', fontWeight: 600 }}>acertou</span>
          </div>
        </div>

        <Label>Como você pontuou</Label>
        <div className="bb-card" style={{ padding: 0, overflow: 'hidden' }}>
          <div style={{ padding: '12px 14px', display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              fontFamily: 'var(--font-display)', fontSize: 22, fontWeight: 700,
              width: 40, color: 'var(--success)',
              fontVariantNumeric: 'tabular-nums',
            }}>20</div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: 14, fontWeight: 500 }}>Placar exato</div>
              <div style={{ fontSize: 12, color: 'var(--fg-2)' }}>Faixa 1 · tempo regulamentar</div>
            </div>
            <div style={{ color: 'var(--success)' }}>{BBIcons.check}</div>
          </div>
          <Div />
          <div style={{ padding: '12px 14px', display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              fontFamily: 'var(--font-display)', fontSize: 22, fontWeight: 700,
              width: 40, color: 'var(--brand-strong)',
              fontVariantNumeric: 'tabular-nums',
            }}>+3</div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: 14, fontWeight: 500 }}>Bônus pênaltis</div>
              <div style={{ fontSize: 12, color: 'var(--fg-2)' }}>Brasil venceu a disputa</div>
            </div>
            <div style={{ color: 'var(--success)' }}>{BBIcons.check}</div>
          </div>
          <div style={{
            padding: '14px',
            background: 'color-mix(in oklab, var(--brand-strong) 14%, transparent)',
            borderTop: '1px solid var(--line)',
            display: 'flex', alignItems: 'center',
          }}>
            <div style={{ fontSize: 14, fontWeight: 600 }}>Total nesta partida</div>
            <div style={{ flex: 1 }}/>
            <div style={{
              fontFamily: 'var(--font-display)', fontSize: 26, fontWeight: 700,
              fontVariantNumeric: 'tabular-nums', letterSpacing: '-0.02em',
              color: 'var(--brand-strong)',
            }}>23 pts</div>
          </div>
        </div>

        <Label>Nos bolões</Label>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {[
            { name: 'Família Silva 2026', delta: '+23', from: '#3', to: '#2' },
            { name: 'Trampo TechCo', delta: '+23', from: '#1', to: '#1' },
            { name: 'Galera do FC', delta: '+23', from: '#5', to: '#3' },
          ].map(p => (
            <div key={p.name} style={{
              display: 'flex', alignItems: 'center', gap: 10,
              padding: '10px 12px',
              background: 'var(--bg-elev)',
              border: '1px solid var(--line)',
              borderRadius: 10,
            }}>
              <div style={{ color: 'var(--fg-2)' }}>{BBIcons.trophy}</div>
              <div style={{ flex: 1, fontSize: 13, fontWeight: 500 }}>{p.name}</div>
              <div style={{ fontSize: 12, color: 'var(--fg-2)', fontVariantNumeric: 'tabular-nums' }}>{p.from} → <b style={{ color: 'var(--fg)' }}>{p.to}</b></div>
              <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--success)', fontVariantNumeric: 'tabular-nums' }}>{p.delta}</div>
            </div>
          ))}
        </div>
      </div>
    </Phone>
  );
}

/* ════════════════════════════════════════════════════════════
   10) ADMIN — Entrada manual de resultado (PRD 4.11)
   ════════════════════════════════════════════════════════════ */
function ScreenAdmin({ theme = 'dark' }) {
  return (
    <Phone theme={theme}>
      <div className="bb-header">
        <div className="bb-iconbtn">{BBIcons.chevronL}</div>
        <div style={{ flex: 1 }}>
          <div style={{ fontSize: 11, color: 'var(--warning)', letterSpacing: '0.08em', textTransform: 'uppercase', fontWeight: 600 }}>Admin de plataforma</div>
          <div className="bb-header-title" style={{ fontSize: 17, marginTop: -2 }}>Resultado manual</div>
        </div>
      </div>

      <div className="bb-scroll">
        <div style={{
          padding: 12,
          background: 'color-mix(in oklab, var(--warning) 12%, transparent)',
          border: '1px solid color-mix(in oklab, var(--warning) 35%, transparent)',
          borderRadius: 12, fontSize: 12, color: 'var(--fg-2)',
          display: 'flex', gap: 10, marginBottom: 14, lineHeight: 1.5,
        }}>
          <div style={{ color: 'var(--warning)' }}>{BBIcons.alert}</div>
          <div>
            Esta alteração é <b style={{ color: 'var(--fg)' }}>global</b> e afeta todos os bolões.
            O provedor de dados sobrescreverá este valor quando o feed for atualizado.
          </div>
        </div>

        <div className="bb-card" style={{ padding: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 11, color: 'var(--fg-3)', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            <span>Partida</span>
            <span style={{ marginLeft: 'auto', fontFamily: 'var(--font-mono)', fontSize: 10, color: 'var(--fg-3)' }}>ID 2026-KO-17</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12, marginTop: 10 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Flag code="BRA" size={30} />
              <div>
                <div style={{ fontSize: 14, fontWeight: 500 }}>Brasil</div>
                <div style={{ fontSize: 11, color: 'var(--fg-3)' }}>mandante</div>
              </div>
            </div>
            <span style={{ color: 'var(--fg-3)' }}>vs</span>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontSize: 14, fontWeight: 500 }}>Uruguai</div>
                <div style={{ fontSize: 11, color: 'var(--fg-3)' }}>visitante</div>
              </div>
              <Flag code="URU" size={30} />
            </div>
          </div>
          <div style={{ marginTop: 10, fontSize: 11, color: 'var(--fg-3)', display: 'flex', gap: 10 }}>
            <span>Oitavas · MetLife Stadium</span>
            <span style={{ marginLeft: 'auto', fontVariantNumeric: 'tabular-nums' }}>Início: 29/06 · 13:00 UTC-3</span>
          </div>
        </div>

        <Label>Placar ao fim do tempo regulamentar</Label>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14 }}>
          <ScoreBox value="1" editable size="lg" />
          <div style={{ fontSize: 22, color: 'var(--fg-3)' }}>×</div>
          <ScoreBox value="1" editable size="lg" />
        </div>

        <Label>Disputa de pênaltis</Label>
        <div style={{ display: 'flex', gap: 8 }}>
          <div style={{
            flex: 1, padding: 12, borderRadius: 10,
            background: 'color-mix(in oklab, var(--brand-strong) 16%, transparent)',
            border: '1.5px solid var(--brand-strong)',
            display: 'flex', alignItems: 'center', gap: 10,
          }}>
            <div style={{ color: 'var(--brand-strong)' }}>{BBIcons.check}</div>
            <div style={{ fontSize: 13, fontWeight: 600 }}>Houve pênaltis</div>
          </div>
          <div style={{
            flex: 1, padding: 12, borderRadius: 10,
            background: 'var(--bg-elev)',
            border: '1.5px solid var(--line)',
            display: 'flex', alignItems: 'center', gap: 10, color: 'var(--fg-2)',
          }}>
            <div style={{ fontSize: 13 }}>Não houve</div>
          </div>
        </div>

        <Label>Vencedor na disputa</Label>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
          {[{ c: 'BRA', n: 'Brasil', pk: '4' }, { c: 'URU', n: 'Uruguai', pk: '3' }].map((t, i) => (
            <div key={t.c} style={{
              padding: 12, borderRadius: 10,
              background: i === 0 ? 'color-mix(in oklab, var(--brand-strong) 14%, transparent)' : 'var(--bg-elev)',
              border: '1.5px solid ' + (i === 0 ? 'var(--brand-strong)' : 'var(--line)'),
              display: 'flex', alignItems: 'center', gap: 10,
            }}>
              <Flag code={t.c} size={26} />
              <div style={{ flex: 1, fontSize: 13, fontWeight: i === 0 ? 600 : 500 }}>{t.n}</div>
              <div style={{ fontSize: 13, fontFamily: 'var(--font-display)', fontWeight: 700, fontVariantNumeric: 'tabular-nums' }}>{t.pk}</div>
            </div>
          ))}
        </div>

        <div style={{
          marginTop: 16, padding: 12,
          border: '1px solid var(--line)',
          borderRadius: 12, fontSize: 11, color: 'var(--fg-3)',
          display: 'flex', flexDirection: 'column', gap: 6,
        }}>
          <div style={{ display: 'flex' }}>
            <span style={{ width: 120 }}>Operador</span>
            <b style={{ color: 'var(--fg-2)' }}>admin@bigball.app</b>
          </div>
          <div style={{ display: 'flex' }}>
            <span style={{ width: 120 }}>Origem vigente</span>
            <b style={{ color: 'var(--warning)' }}>manual (pendente feed)</b>
          </div>
          <div style={{ display: 'flex' }}>
            <span style={{ width: 120 }}>Última sync feed</span>
            <b style={{ color: 'var(--fg-2)' }}>há 38 min · erro 502</b>
          </div>
        </div>

        <div style={{ height: 14 }}/>
        <button className="bb-btn bb-btn-primary" style={{ width: '100%', background: 'var(--warning)', color: '#1a1408' }}>
          Confirmar e recalcular rankings
        </button>
      </div>
    </Phone>
  );
}

Object.assign(window, {
  ScreenLogin, ScreenHome, ScreenPool, ScreenPredictGroup,
  ScreenPredictKO, ScreenCalendar, ScreenCreatePool,
  ScreenJoinCode, ScreenMatchResult, ScreenAdmin,
});
