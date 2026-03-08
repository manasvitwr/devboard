// ── Sortable initializer ──────────────────────────────────────────────────────
function initSortable() {
    document.querySelectorAll('.kanban-column').forEach(function (column) {
        if (column._sortableInited) return;
        column._sortableInited = true;
        new Sortable(column, {
            group: 'kanban',
            animation: 150,
            ghostClass: 'dragging',
            onEnd: function (evt) {
                var ticketId = evt.item.getAttribute('data-ticket-id');
                var newStatus = evt.to.getAttribute('data-status');
                var fd = new FormData();
                fd.append('action', 'updateStatus');
                fd.append('ticketId', ticketId);
                fd.append('status', newStatus);
                fetch('TicketHandler.ashx', { method: 'POST', body: fd })
                    .then(function (r) { return r.json(); })
                    .then(function (d) {
                        if (!d.success) console.error('Status update failed:', d.message);
                    })
                    .catch(function (e) { console.error('Status update error:', e); });
            }
        });
    });
}

// ── Vote button visual state ──────────────────────────────────────────────────
// userVote: 1 = solid green up, -1 = solid red down, 0 = both outline
function setVoteButtonState(upBtn, downBtn, userVote) {
    var uv = Number(userVote);
    upBtn.classList.remove('btn-success', 'btn-outline-success');
    downBtn.classList.remove('btn-danger', 'btn-outline-danger');
    if (uv === 1) {
        upBtn.classList.add('btn-success');
        downBtn.classList.add('btn-outline-danger');
    } else if (uv === -1) {
        upBtn.classList.add('btn-outline-success');
        downBtn.classList.add('btn-danger');
    } else {
        upBtn.classList.add('btn-outline-success');
        downBtn.classList.add('btn-outline-danger');
    }
}

// Called on page load + every UpdatePanel postback
function initVoteStates() {
    document.querySelectorAll('.vote-section').forEach(function (section) {
        var upBtn = section.querySelector('[data-value="1"]');
        var downBtn = section.querySelector('[data-value="-1"]');
        if (!upBtn || !downBtn) return;
        setVoteButtonState(upBtn, downBtn, section.getAttribute('data-user-vote') || '0');
    });
}

function pageReady() {
    initVoteStates();
    initSortable();
}

// ScriptManager emits scripts at bottom of <body> — DOM ready at parse time
pageReady();

if (typeof Sys !== 'undefined' && Sys.Application) {
    Sys.Application.add_load(pageReady);
}

// ── Vote click ────────────────────────────────────────────────────────────────
document.addEventListener('click', function (e) {
    var button = e.target.closest('.vote-btn');
    if (!button) return;

    e.preventDefault();
    e.stopPropagation();

    // Capture DOM refs synchronously
    var section = button.closest('.vote-section');
    var upBtn = section.querySelector('[data-value="1"]');
    var downBtn = section.querySelector('[data-value="-1"]');
    var scoreEl = section.querySelector('.badge');
    var ticketId = button.getAttribute('data-ticket-id');
    var clicked = Number(button.getAttribute('data-value')); // 1 or -1
    var prevVote = Number(section.getAttribute('data-user-vote') || '0');

    // ── Optimistic update (instant, before server confirms) ──────────────────
    // Toggle off if clicking the same direction, otherwise switch
    var optimistic = (prevVote === clicked) ? 0 : clicked;
    setVoteButtonState(upBtn, downBtn, optimistic);
    section.setAttribute('data-user-vote', String(optimistic));

    var fd = new FormData();
    fd.append('action', 'vote');
    fd.append('ticketId', ticketId);
    fd.append('value', String(clicked));

    fetch('TicketHandler.ashx', { method: 'POST', body: fd })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (!data.success) {
                // Revert optimistic update on error
                console.error('Vote failed:', data.message);
                setVoteButtonState(upBtn, downBtn, prevVote);
                section.setAttribute('data-user-vote', String(prevVote));
                return;
            }
            // Confirm with server's authoritative value + update score
            var confirmed = Number(data.userVote);
            setVoteButtonState(upBtn, downBtn, confirmed);
            section.setAttribute('data-user-vote', String(confirmed));
            if (scoreEl) scoreEl.textContent = data.score;
        })
        .catch(function (err) {
            // Revert on network error
            console.error('Vote error:', err);
            setVoteButtonState(upBtn, downBtn, prevVote);
            section.setAttribute('data-user-vote', String(prevVote));
        });
});
