// Initialize SortableJS for Kanban board
document.addEventListener('DOMContentLoaded', function () {
    var kanbanColumns = document.querySelectorAll('.kanban-column');

    kanbanColumns.forEach(function (column) {
        new Sortable(column, {
            group: 'kanban',
            animation: 150,
            ghostClass: 'dragging',
            onEnd: function (evt) {
                var ticketId = evt.item.getAttribute('data-ticket-id');
                var newStatus = evt.to.getAttribute('data-status');

                // Send AJAX request to update ticket status
                var formData = new FormData();
                formData.append('action', 'updateStatus');
                formData.append('ticketId', ticketId);
                formData.append('status', newStatus);

                fetch('TicketHandler.ashx', {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    if (!data.success) {
                        console.error('Failed to update ticket status:', data.message);
                        // Optionally revert the drag
                    }
                })
                .catch(error => {
                    console.error('Error updating ticket status:', error);
                });
            }
        });
    });

    // Vote button handlers
    document.addEventListener('click', function (e) {
        if (e.target.closest('.vote-btn')) {
            var button = e.target.closest('.vote-btn');
            var ticketId = button.getAttribute('data-ticket-id');
            var value = button.getAttribute('data-value');

            var formData = new FormData();
            formData.append('action', 'vote');
            formData.append('ticketId', ticketId);
            formData.append('value', value);

            fetch('TicketHandler.ashx', {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the score display
                    var scoreElement = button.parentElement.querySelector('.badge');
                    if (scoreElement) {
                        scoreElement.textContent = data.score;
                    }
                } else {
                    console.error('Failed to vote:', data.message);
                }
            })
            .catch(error => {
                console.error('Error voting:', error);
            });
        }
    });
});
