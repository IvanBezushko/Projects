import React from 'react';

function Progress_list({ tasks, removeTask, moveTask }) {
  const handleDragStart = (e, taskId) => {
    e.dataTransfer.setData('taskId', taskId); // Przechowujemy ID zadania podczas przeciągania
    e.dataTransfer.setData('from', 'progress'); // Przechowujemy, z której listy przenosimy
  };

  const handleDrop = (e) => {
    const taskId = e.dataTransfer.getData('taskId');
    const from = e.dataTransfer.getData('from');
    console.log(`Przenoszenie zadania o ID ${taskId} z ${from} do Progress`);
    moveTask(parseInt(taskId), from, 'progress'); // Przenosimy zadanie z dowolnej listy do Progress
  };

  const handleDragOver = (e) => {
    e.preventDefault(); // Umożliwia upuszczanie na tę listę
  };

  return (
    <div className="progress-list-container" onDrop={handleDrop} onDragOver={handleDragOver}>
      
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

export default Progress_list;
