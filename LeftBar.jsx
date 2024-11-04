import React, { useState } from 'react';
import './LeftBar.css';

function LeftBar({ addTask }) {
  const [task, setTask] = useState('');
  const [showInput, setShowInput] = useState(false); // Stan kontrolujący widoczność pola tekstowego

  const handleSubmit = (event) => {
    event.preventDefault();
    if (task.trim()) {
      addTask(task); // Dodaj zadanie, jeśli nie jest puste
      setTask(''); // Wyczyść pole tekstowe po dodaniu zadania
      setShowInput(false); // Schowaj pole po dodaniu zadania
    }
  };

  const handleButtonClick = () => {
    setShowInput(!showInput); // Zmień widoczność pola tekstowego po kliknięciu przycisku
  };

  return (
    <div className="container">
      <div className="todo-box">
        <p>
          To<br />
          <span className="larger-text">List</span>
          <br />
          Do
        </p>
      </div>
      <div className="add-button">
        <button onClick={handleButtonClick} className='plus'>+</button> {/* Przycisk pokazujący/ukrywający pole tekstowe */}
      </div>
      {/* Jeśli showInput jest true, pokaż pole tekstowe */}
      {showInput && (
        <form onSubmit={handleSubmit}>
          <input
            type="text"
            value={task}
            onChange={(e) => setTask(e.target.value)}
            placeholder="Enter new task"
          />
          
        </form>
      )}
    </div>
  );
}

export default LeftBar;
