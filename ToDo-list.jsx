import React from 'react';

function ToDo_list({ tasks, removeTask, moveTask }) {
  const handleDragStart = (e, taskId) => {
    e.dataTransfer.setData('taskId', taskId); // Przechowujemy ID zadania podczas przeciągania
    e.dataTransfer.setData('from', 'todo'); // Przechowujemy, z której listy przenosimy
  };

  const handleDrop = (e) => {
    const taskId = e.dataTransfer.getData('taskId');
    const from = e.dataTransfer.getData('from');
    console.log(`Przenoszenie zadania o ID ${taskId} z ${from} do ToDo`);
    moveTask(parseInt(taskId), from, 'todo'); // Przenosimy zadanie z dowolnej listy do ToDo
  };

  const handleDragOver = (e) => {
    e.preventDefault(); // Umożliwia upuszczanie na tę listę
  };

  return (
    <div className="todo-list-container" onDrop={handleDrop} onDragOver={handleDragOver}>
      
      <ul>
        {tasks.length === 0 ? (
          <p></p>
        ) : (
          tasks.map((task) => (
            <li key={task.id} draggable onDragStart={(e) => handleDragStart(e, task.id)}>
              <p>{task.text}</p>
              <button onClick={() => removeTask(task.id)}>✖</button>
            </li>
          ))
        )}
      </ul>
    </div>
  );
}

export default ToDo_list;
