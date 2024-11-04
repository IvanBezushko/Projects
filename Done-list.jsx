import React from 'react';

function Done_list({ tasks, removeTask, moveTask }) {
  const handleDragStart = (e, taskId) => {
    e.dataTransfer.setData('taskId', taskId); // Przechowujemy ID zadania podczas przeciągania
    e.dataTransfer.setData('from', 'done'); // Przechowujemy, z której listy przenosimy
  };

  const handleDrop = (e) => {
    const taskId = e.dataTransfer.getData('taskId');
    const from = e.dataTransfer.getData('from');
    console.log(`Przenoszenie zadania o ID ${taskId} z ${from} do Done`);
    moveTask(parseInt(taskId), from, 'done'); // Przenosimy zadanie z dowolnej listy do Done
  };

  const handleDragOver = (e) => {
    e.preventDefault(); // Umożliwia upuszczanie na tę listę
  };

  return (
    <div className="done-list-container" onDrop={handleDrop} onDragOver={handleDragOver}>
      
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

export default Done_list;
