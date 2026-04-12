import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import { getTodos, createTodo, updateTodo, deleteTodo } from '../api/todos'

export default function TodosPage() {
  const { token, logout } = useAuth()
  const [todos, setTodos] = useState([])
  const [newTitle, setNewTitle] = useState('')
  const [loading, setLoading] = useState(true)
  const [adding, setAdding] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    getTodos(token)
      .then(setTodos)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }, [token])

  async function handleAdd(e) {
    e.preventDefault()
    const title = newTitle.trim()
    if (!title) return
    setAdding(true)
    try {
      const todo = await createTodo(token, title)
      setTodos(prev => [...prev, todo])
      setNewTitle('')
    } catch (err) {
      setError(err.message)
    } finally {
      setAdding(false)
    }
  }

  async function handleToggle(todo) {
    // Optimistic update
    setTodos(prev =>
      prev.map(t => t.id === todo.id ? { ...t, isDone: !t.isDone } : t)
    )
    try {
      await updateTodo(token, todo.id, todo.title, !todo.isDone)
    } catch {
      // Revert on failure
      setTodos(prev =>
        prev.map(t => t.id === todo.id ? { ...t, isDone: todo.isDone } : t)
      )
      setError('Failed to update todo.')
    }
  }

  async function handleDelete(id) {
    setTodos(prev => prev.filter(t => t.id !== id))
    try {
      await deleteTodo(token, id)
    } catch {
      // Revert on failure — refetch to restore accurate state
      getTodos(token).then(setTodos)
      setError('Failed to delete todo.')
    }
  }

  const pending = todos.filter(t => !t.isDone)
  const done = todos.filter(t => t.isDone)

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <header className="bg-white border-b border-gray-200">
        <div className="max-w-xl mx-auto px-4 h-14 flex items-center justify-between">
          <span className="font-semibold text-gray-800">Ballast</span>
          <button
            onClick={logout}
            className="text-sm text-gray-500 hover:text-gray-800 transition-colors"
          >
            Sign out
          </button>
        </div>
      </header>

      <main className="max-w-xl mx-auto px-4 py-8 space-y-6">
        {/* Add todo form */}
        <form onSubmit={handleAdd} className="flex gap-2">
          <input
            type="text"
            placeholder="Add a new todo…"
            value={newTitle}
            onChange={e => setNewTitle(e.target.value)}
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <button
            type="submit"
            disabled={adding || !newTitle.trim()}
            className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
          >
            {adding ? 'Adding…' : 'Add'}
          </button>
        </form>

        {error && (
          <p className="text-sm text-red-600">{error}</p>
        )}

        {/* List */}
        {loading ? (
          <p className="text-sm text-gray-400 text-center">Loading…</p>
        ) : todos.length === 0 ? (
          <p className="text-sm text-gray-400 text-center">No todos yet. Add one above!</p>
        ) : (
          <div className="space-y-4">
            {/* Pending */}
            {pending.length > 0 && (
              <ul className="bg-white rounded-2xl shadow-sm divide-y divide-gray-100">
                {pending.map(todo => (
                  <TodoItem key={todo.id} todo={todo} onToggle={handleToggle} onDelete={handleDelete} />
                ))}
              </ul>
            )}

            {/* Done */}
            {done.length > 0 && (
              <div>
                <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-2 px-1">
                  Completed
                </p>
                <ul className="bg-white rounded-2xl shadow-sm divide-y divide-gray-100">
                  {done.map(todo => (
                    <TodoItem key={todo.id} todo={todo} onToggle={handleToggle} onDelete={handleDelete} />
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  )
}

function TodoItem({ todo, onToggle, onDelete }) {
  return (
    <li className="flex items-center gap-3 px-4 py-3">
      <input
        type="checkbox"
        checked={todo.isDone}
        onChange={() => onToggle(todo)}
        className="w-4 h-4 rounded accent-blue-600 cursor-pointer"
      />
      <span className={`flex-1 text-sm ${todo.isDone ? 'line-through text-gray-400' : 'text-gray-800'}`}>
        {todo.title}
      </span>
      <button
        onClick={() => onDelete(todo.id)}
        aria-label="Delete"
        className="text-gray-300 hover:text-red-400 transition-colors text-lg leading-none"
      >
        ×
      </button>
    </li>
  )
}
