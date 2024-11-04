import './App.css';
import LeftBar from './LeftBar';
import ToDo from './ToDo';
import Progress from './Progress';
import Done from './Done';
import ToDo_list from './ToDo-list';
import Progress_list from './Progress-list';
import Done_list from './Done-list';
import { useState, useEffect } from 'react';

function App() {
  const [tasks, setTasks] = useState(() => {
    const savedTasks = localStorage.getItem('tasks');
    console.log('Pobranie zadań z localStorage:', savedTasks);
    return savedTasks
      ? JSON.parse(savedTasks)
      : {
          todo: [],
          progress: [],
          done: [],
        };
  });

  useEffect(() => {
    console.log('Zapis zadań do localStorage:', tasks);
    localStorage.setItem('tasks', JSON.stringify(tasks));
  }, [tasks]);

  const addTask = (task) => {
    setTasks((prev) => ({
      ...prev,
      todo: [...prev.todo, { id: Date.now(), text: task }],
    }));
  };

  const removeTask = (id, list) => {
    setTasks((prev) => ({
      ...prev,
      [list]: prev[list].filter((task) => task.id !== id),
    }));
  };

  const moveTask = (id, from, to) => {
    if (from === to) {
      console.log(`Zadanie o ID ${id} już znajduje się na liście ${from}, brak potrzeby przenoszenia.`);
      return; // Jeśli zadanie jest przenoszone na tę samą listę, zatrzymujemy operację
    }
  
    console.log(`Przenoszenie zadania o ID ${id} z ${from} do ${to}`);
  
    setTasks((prev) => {
      // Znajdź zadanie w liście "from"
      const taskToMove = prev[from].find((task) => task.id === id);
      if (!taskToMove) {
        console.error(`Nie znaleziono zadania z ID: ${id} w liście: ${from}`);
        return prev; // Jeśli zadanie nie istnieje, zatrzymujemy operację
      }
  
      // Usuń zadanie z listy "from"
      const updatedFromList = prev[from].filter((task) => task.id !== id);
  
      // Dodaj zadanie do listy "to"
      const updatedToList = [...prev[to], taskToMove];
  
      console.log(`Zadanie "${taskToMove.text}" przeniesione z ${from} do ${to}`);
      console.log('Zaktualizowana lista zadań:', {
        ...prev,
        [from]: updatedFromList,
        [to]: updatedToList,
      });
  
      return {
        ...prev,
        [from]: updatedFromList,
        [to]: updatedToList,
      };
    });
  };
  
  

  return (
    <div className="App">
      <LeftBar addTask={addTask} />
      <div className="kanban-board">
        <ToDo />
        <Progress />
        <Done />
      </div>
      <div className="board-list">
        <ToDo_list tasks={tasks.todo} removeTask={(id) => removeTask(id, 'todo')} moveTask={moveTask} />
        <Progress_list tasks={tasks.progress} removeTask={(id) => removeTask(id, 'progress')} moveTask={moveTask} />
        <Done_list tasks={tasks.done} removeTask={(id) => removeTask(id, 'done')} moveTask={moveTask} />
      </div>
    </div>
  );
}

export default App;
