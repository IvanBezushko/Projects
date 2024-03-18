//Przycisk "Więcej"
$('button.przycisk').on('click', function() {
    $('#loader').show();
    setTimeout(function() {
      $('#loader').hide();
    }, 2000); 
  });


  //pomóc w scrollowaniu strony
  $(window).scroll(function() {
    if ($(window).scrollTop() > 300) {
      btn.addClass('show');
    } else {
      btn.removeClass('show');
    }
  });
 


  //animacja spadającej sakury
  function createPetal() {
    const sakuraContainer = document.getElementById('sakura');
    const petal = document.createElement('div');
    petal.classList.add('petal');
    petal.style.left = Math.random() * 100 + 'vw';
    petal.style.animationDuration = Math.random() * 5 + 3 + 's'; // Od 3 do 8 sekund
    petal.style.opacity = Math.random();
    petal.style.transform = `scale(${Math.random()})`;
    petal.style.animationName = 'fall';
    petal.style.animationTimingFunction = 'ease-in';
    sakuraContainer.appendChild(petal);
    setTimeout(() => {
      petal.remove();
    }, 5000); 
  }
  setInterval(createPetal, 50);
  


  //Animacja słońca
  document.addEventListener("DOMContentLoaded", function() {
    const sun = document.getElementById('sun');
    function animateSunrise() {
        let bottomPosition = -50;
        function frame() {
            bottomPosition += 1; 
            sun.style.bottom = bottomPosition + 'px'; 
            if (bottomPosition == 300) { 
                clearInterval(id);
            }
        }
        let id = setInterval(frame, 5); 
    }
    animateSunrise();
});



//Konwerter waluty
async function getExchangeRate(fromCurrency, toCurrency) {
  try {
      const response = await fetch(`https://api.exchangerate-api.com/v4/latest/${fromCurrency}`);
      const data = await response.json();
      return data.rates[toCurrency];
  } catch (error) {
      console.error("Błąd podczas pobierania kursu waluty: ", error);
      return null;
  }
}
async function convertCurrency() {
  const fromCurrency = document.getElementById('from-currency').value;
  const toCurrency = document.getElementById('to-currency').value;
  const amount = document.getElementById('amount').value;
  const rate = await getExchangeRate(fromCurrency, toCurrency);
  if (rate !== null) {
      const result = amount * rate;
      document.getElementById('result').innerText = `${amount} ${fromCurrency} = ${result.toFixed(2)} ${toCurrency}`;
  } else {
      document.getElementById('result').innerText = 'Nie udało się przeliczyć waluty.';
  }
}



//ChatBot
function sendMessage() {
  var inputElement = document.getElementById("chatbot-input");
  var message = inputElement.value.trim();
  inputElement.value = "";
  if (message !== "") {
      displayMessage("Ty: " + message, "user");
      getBotResponse(message);
  }
}

function displayMessage(message, sender) {
  var chatbotMessages = document.getElementById("chatbot-messages");
  var messageDiv = document.createElement("div");
  if(sender === "user") {
      messageDiv.style.textAlign = "right";
      messageDiv.style.backgroundColor = "#e0f7fa"; // Light blue for user
  } else {
      messageDiv.style.textAlign = "left";
      messageDiv.style.backgroundColor = "#e1bee7"; // Light purple for bot
  }
  messageDiv.style.borderRadius = "20px";
  messageDiv.style.margin = "5px";
  messageDiv.style.padding = "5px 10px";
  messageDiv.textContent = message;

  chatbotMessages.appendChild(messageDiv);
  chatbotMessages.scrollTop = chatbotMessages.scrollHeight; // Scroll to the bottom
}

function getBotResponse(message) {
  var response = "";
  var msgLower = message.toLowerCase();

  if (msgLower.includes("wiza do japonii") || msgLower === "jak uzyskać wizę do japonii?") {
      response = "Obywatele wielu krajów mogą wjechać do Japonii bez wizy na krótkie pobyty.";
  } else if (msgLower.includes("waluta w japonii") || msgLower === "jaka jest waluta w japonii?") {
      response = "W Japonii używa się jena (JPY).";
  } else if (msgLower.includes("pogoda w japonii") || msgLower === "jakie jest pogoda w japonii?") {
      response = "Pogoda w Japonii jest zróżnicowana w zależności od regionu i pory roku. Możesz ją sprawdzić u nas na stronie.";
  } else if (msgLower.includes("jedzenie w japonii") || msgLower === "co warto zjeść będąc w japonii?") {
      response = "Japońska kuchnia jest różnorodna, popularne są sushi, ramen i tempura. Polecam poszukać coś dla siebie w restauracjach przy targowych ulicach.";
  } else if (msgLower.includes("transport w japonii") || msgLower === "jak wygląda transport w japonii?") {
      response = "W Japonii jest dobrze rozwinięta sieć transportu publicznego, w tym pociągi Shinkansen, ale trochę kosztuje.";
  } else if (msgLower.includes("atrakcje turystyczne w japonii") || msgLower === "jakie są popularne atrakcje turystyczne w japonii?") {
      response = "Popularne atrakcje to m.in. góra Fuji, świątynie w Kioto i tokijskie dzielnice Shibuya i Shinjuku.";
  } else if (msgLower.includes("kultura japonii") || msgLower === "chcę dowiedzieć się więcej o kulturze japonii.") {
      response = "Japonia jest znana z bogatej kultury, w tym sztuki, teatru kabuki, ceremonii parzenia herbaty i festiwale.";
  } else if (msgLower.includes("zakwaterowanie w japonii") || msgLower === "gdzie mogę znaleźć zakwaterowanie w japonii?") {
      response = "Możliwości zakwaterowania w Japonii obejmują hotele, ryokany oraz hostele.";
  } else if (msgLower.includes("bezpieczeństwo w japonii") || msgLower === "czy japonia jest bezpiecznym krajem?") {
      response = "Japonia jest uważana za jeden z bezpieczniejszych krajów do odwiedzenia.";
  } else if (msgLower.includes("festiwale w japonii") || msgLower === "jakie festiwale odbywają się w japonii?") {
      response = "W Japonii odbywa się wiele festiwali, takich jak Hanami (kwitnące wiśnie) czy Matsuri oraz festiwal fajerwerków w ostatni dzień lata.";
  } else if (msgLower.includes("kurs waluty w japonii") || msgLower === "gdzie mogę sprawdzić aktualny kurs waluty?") {
      response = "Aktualny kurs wymiany walut możesz sprawdzić u nas na stronie trochę wyżej.";
  } else {
      response = "Przepraszam, nie rozumiem pytania. Proszę zadać inne pytanie.";
  }

  displayMessage("Bot: " + response, "bot");
}

function sendQuestion(question) {
  document.getElementById("chatbot-input").value = question;
  sendMessage();
}







//Animacja scrollowanie w navigacji(JQuery)
$(document).ready(function(){
  $('nav a').click(function(event){
    event.preventDefault();
    var targetSection = $(this).attr('href');
    $('html, body').animate({
      scrollTop: $(targetSection).offset().top
    }, 1000);
  });

  $(window).on('hashchange', function() {
    var targetSection = window.location.hash;
    $(targetSection).addClass('active').siblings().removeClass('active');
  });
});



//Data i Czas (JQuery)
$(document).ready(function() {
    function aktualizujCzasIData() {
        var teraz = new Date();
        var godziny = teraz.getHours();
        var minuty = teraz.getMinutes();
        var sekundy = teraz.getSeconds();
        minuty = (minuty < 10 ? "0" : "") + minuty;
        sekundy = (sekundy < 10 ? "0" : "") + sekundy;

        var czasStr = godziny + ":" + minuty + ":" + sekundy;
        $('#zegar').text(czasStr);

        var dataStr = teraz.toLocaleDateString('pl-PL');
        $('#data').text(dataStr);
    }
    setInterval(aktualizujCzasIData, 1000);
});



//Slider (JQuery)
$(document).ready(function() {
    var slideIndex = 0;
    pokazSlajdy();

    function pokazSlajdy() {
        var i;
        var slides = $(".slide");
        for (i = 0; i < slides.length; i++) {
           slides.eq(i).removeClass("aktywny");
        }
        slideIndex++;
        if (slideIndex > slides.length) {slideIndex = 1} 
        slides.eq(slideIndex - 1).addClass("aktywny");
        setTimeout(pokazSlajdy, 3000); // Zmień obraz co 3 sekundy
    }
});


  
//Animacja scrollowania w górę(JQuery)
    jQuery(document).ready(function() {
  
        var btn = $('#button');
      
        $(window).scroll(function() {
          if ($(window).scrollTop() > 300) {
            btn.addClass('show');
          } else {
            btn.removeClass('show');
          }
        });
      
        btn.on('click', function(e) {
          e.preventDefault();
          $('html, body').animate({scrollTop:0}, '300');
        });
      
      });
      var btn = $('#button');


      
  //Pogoda (JQuery)
      $(document).ready(function() {
        function getWeather(city) {
          var apiKey = '5af3a85cd9c526e0f74d6a8eba27a846';
          var url = `http://api.openweathermap.org/data/2.5/weather?q=${city}&appid=${apiKey}&units=metric`;
      
          $.get(url, function(data) {
            $('#temp').text(data.main.temp);
            $('#conditions').text(data.weather[0].main);
            $('#humidity').text(data.main.humidity);
            $('#wind-speed').text(data.wind.speed);
            $('#wind-direction').text(getWindDirection(data.wind.deg));
            $('#pressure').text(data.main.pressure);
          });
        }
      
        function getWindDirection(degree) {
          if (degree > 45 && degree <= 135) {
            return 'Wschód';
          } else if (degree > 135 && degree <= 225) {
            return 'Południe';
          } else if (degree > 225 && degree <= 315) {
            return 'Zachód';
          } else {
            return 'Północ';
          }
        }
      
        $('#city-selector').change(function() {
          var selectedCity = $(this).val();
          getWeather(selectedCity);
        });
      
        getWeather($('#city-selector').val());
      });
      


      //AJAX, obsługa tabeli + JQuery
      $(document).ready(function() {
        $.ajax({
            url: 'tabela.json', 
            type: 'GET',
            dataType: 'json',
            success: function(data) {
                var table = '<table>';
                
                table += '<thead><tr><th>Miasto</th><th>Economic</th><th>Premium</th><th>Razem</th></tr></thead>';
                table += '<tbody>';
                
                $.each(data, function(key, value) {
                    table += '<tr>';
                    table += '<td>' + value.city + '</td>';
                    table += '<td>' + value.flights.Economic + ' zł - ' + value.hotels.Economic + ' zł(7 dni)</td>';
                    table += '<td>' + value.flights.Premium + ' zł - ' + value.hotels.Premium + ' zł(7 dni)</td>';
                    table += '<td>Economic: ' + value.total.Economic + ' zł - Premium: ' + value.total.Premium + ' zł</td>';
                    table += '</tr>';
                });
    
                table += '</tbody></table>';
                $('#table-container').html(table);
            }
        });
    });
    
  

      
      
  
















