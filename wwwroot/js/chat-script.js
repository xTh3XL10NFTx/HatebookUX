// Make an AJAX request to load the chat button HTML content
function loadChatButton() {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Shared/chat-button.html', true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE && xhr.status === 200) {
            var chatButtonHtml = xhr.responseText;

            // Insert the chat button HTML content into the desired location on the page
            var chatContainer = document.getElementById('chat-container');
            chatContainer.innerHTML = chatButtonHtml;
        }
    };
    xhr.send();
}

// Call the function to load the chat button on page load
document.addEventListener('DOMContentLoaded', loadChatButton);
