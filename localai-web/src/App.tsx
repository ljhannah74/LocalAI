import { useState } from 'react'
import './App.css'

interface TextAnalysisResult {
  sentiment: string
  entities: string[]
  pii: string[]
}

const sampleText =
  'I ordered a laptop from Contoso yesterday, but it arrived damaged. Please contact alex@example.com about my order.'

function App() {
  const [text, setText] = useState('')
  const [result, setResult] = useState<TextAnalysisResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  async function analyzeText() {
    if (!text.trim() || loading) return

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const response = await fetch('/api/textanalysis', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text }),
      })

      if (!response.ok) {
        throw new Error(`Analysis failed (HTTP ${response.status})`)
      }

      const data: TextAnalysisResult = await response.json()
      setResult(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred.')
    } finally {
      setLoading(false)
    }
  }

  function clearText() {
    setText('')
    setResult(null)
    setError('')
  }

  const sentiment = result?.sentiment.trim().toLowerCase() ?? ''
  const sentimentClass = ['positive', 'negative', 'neutral', 'mixed'].includes(sentiment)
    ? sentiment
    : 'unknown'

  return (
    <div className="app-shell">
      <header className="site-header">
        <div className="header-inner">
          <div className="brand">
            <span className="brand-mark" aria-hidden="true">✦</span>
            <span>Local<span className="brand-accent">AI</span></span>
          </div>
          <span className="local-badge"><span className="badge-dot" /> Local inference</span>
        </div>
      </header>

      <main className="page-content">
        <div className="intro">
          <span className="eyebrow">AI-POWERED TEXT INTELLIGENCE</span>
          <h1>Understand your text <span>instantly.</span></h1>
          <p>Analyze sentiment, identify named entities, and flag potential personal information using a locally hosted AI model.</p>
        </div>

        <section className="panel editor-panel" aria-labelledby="input-heading">
          <div className="panel-heading">
            <div>
              <h2 id="input-heading">Text to analyze</h2>
              <p>Paste a passage or try an example to get started.</p>
            </div>
            <button className="text-button" type="button" onClick={() => {
              setText(sampleText)
              setResult(null)
              setError('')
            }} disabled={loading}>Use sample</button>
          </div>

          <label className="sr-only" htmlFor="analysis-input">Text to analyze</label>
          <textarea
            id="analysis-input"
            value={text}
            onChange={(event) => {
              setText(event.target.value)
              setResult(null)
              setError('')
            }}
            placeholder="Enter or paste your text here…"
            rows={8}
            disabled={loading}
          />

          <div className="editor-footer">
            <span className="character-count">{text.length.toLocaleString()} characters</span>
            <div className="editor-actions">
              <button className="secondary-button" type="button" onClick={clearText} disabled={loading || (!text && !result && !error)}>Clear</button>
              <button className="primary-button" type="button" onClick={analyzeText} disabled={loading || !text.trim()}>
                <span aria-hidden="true">✦</span> {loading ? 'Analyzing…' : 'Analyze text'}
              </button>
            </div>
          </div>
        </section>

        {loading && <p className="status-message" role="status">Analyzing your text with the local model. This may take a moment…</p>}
        {error && <div className="error-banner" role="alert">{error}</div>}

        {result && (
          <section className="analysis-section" aria-labelledby="results-heading" aria-live="polite">
            <div className="results-heading">
              <div>
                <span className="eyebrow">ANALYSIS COMPLETE</span>
                <h2 id="results-heading">Your results</h2>
              </div>
              <span className="results-check" aria-hidden="true">✓</span>
            </div>
            <div className="results-grid">
              <article className="result-card sentiment-card">
                <div className="card-title"><span aria-hidden="true">◉</span><h3>Sentiment</h3></div>
                <span className={`sentiment-pill ${sentimentClass}`}>{result.sentiment || 'Unknown'}</span>
                <p className="card-description">Overall tone detected in your text</p>
              </article>
              <article className="result-card">
                <div className="card-title"><span aria-hidden="true">◇</span><h3>Named entities</h3><span className="count-pill">{result.entities.length}</span></div>
                {result.entities.length > 0 ? (
                  <ul className="chip-list">{result.entities.map((entity, index) => <li className="chip" key={`${entity}-${index}`}>{entity}</li>)}</ul>
                ) : <p className="empty-message">No named entities detected.</p>}
              </article>
              <article className="result-card">
                <div className="card-title"><span aria-hidden="true">⬡</span><h3>Potential PII</h3><span className="count-pill">{result.pii.length}</span></div>
                {result.pii.length > 0 ? (
                  <ul className="chip-list">{result.pii.map((item, index) => <li className="chip pii-chip" key={`${item}-${index}`}>{item}</li>)}</ul>
                ) : <p className="empty-message">No potential PII detected.</p>}
              </article>
            </div>
            <p className="disclaimer">AI results may be inaccurate. Do not rely on this tool alone to identify sensitive information.</p>
          </section>
        )}
      </main>

      <footer className="site-footer">Built with React, ASP.NET Core &amp; Ollama <span aria-hidden="true">·</span> Runs locally</footer>
    </div>
  )
}

export default App
