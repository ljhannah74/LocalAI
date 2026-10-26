
import { useState } from 'react'
import './App.css'

interface TextAnalysisResult {
  sentiment: string
  entities: string[]
  pii: string[]
}

function App() {
  const [text, setText] = useState('')
  const [result, setResult] =
    useState<TextAnalysisResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  async function analyzeText() {
    if (!text.trim()) return

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const response = await fetch('/api/textanalysis', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ text }),
      })

      if (!response.ok) {
        throw new Error(
          `Analysis failed (HTTP ${response.status})`
        )
      }

      const data: TextAnalysisResult =
        await response.json()

      setResult(data)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected error occurred.'
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="container">
      <h1>LocalAI Text Analyzer</h1>
      <p>
        Analyze sentiment, named entities, and potential PII
        using a locally hosted AI model.
      </p>

      <textarea
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="Enter text to analyze..."
        rows={7}
      />

      <button
        onClick={analyzeText}
        disabled={loading || !text.trim()}
      >
        {loading ? 'Analyzing...' : 'Analyze Text'}
      </button>

      {error && <p className="error">{error}</p>}

      {result && (
        <section className="results">
          <h2>Analysis Results</h2>

          <h3>Sentiment</h3>
          <p>{result.sentiment}</p>

          <h3>Named Entities</h3>
          <ul>
            {result.entities.map((entity, index) => (
              <li key={index}>{entity}</li>
            ))}
          </ul>

          <h3>Potential PII</h3>
          <ul>
            {result.pii.map((item, index) => (
              <li key={index}>{item}</li>
            ))}
          </ul>
        </section>
      )}
    </main>
  )
}

export default App
