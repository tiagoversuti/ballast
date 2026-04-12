function authHeaders(token) {
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`,
  }
}

export async function getTodos(token) {
  const res = await fetch('/api/todos', { headers: authHeaders(token) })
  if (!res.ok) throw new Error('Failed to load todos.')
  return res.json()
}

export async function createTodo(token, title) {
  const res = await fetch('/api/todos', {
    method: 'POST',
    headers: authHeaders(token),
    body: JSON.stringify({ title }),
  })
  if (!res.ok) throw new Error('Failed to create todo.')
  return res.json()
}

export async function updateTodo(token, id, title, isDone) {
  const res = await fetch(`/api/todos/${id}`, {
    method: 'PUT',
    headers: authHeaders(token),
    body: JSON.stringify({ title, isDone }),
  })
  if (!res.ok) throw new Error('Failed to update todo.')
}

export async function deleteTodo(token, id) {
  const res = await fetch(`/api/todos/${id}`, {
    method: 'DELETE',
    headers: authHeaders(token),
  })
  if (!res.ok) throw new Error('Failed to delete todo.')
}
