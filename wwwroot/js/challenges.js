// Challenges Page JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Elements
    const searchInput = document.getElementById('searchInput');
    const applyFiltersBtn = document.getElementById('applyFilters');
    const resetFiltersBtn = document.getElementById('resetFilters');
    const challengeItems = document.querySelectorAll('.challenge-item');
    const sortLinks = document.querySelectorAll('[data-sort]');
    const bookmarkBtns = document.querySelectorAll('.bookmark-btn');

    // Bookmarks storage (using in-memory storage)
    let bookmarks = [];

    // Initialize bookmarks from memory
    initializeBookmarks();

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterChallenges, 300));
    }

    // Apply filters
    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', filterChallenges);
    }

    // Reset filters
    if (resetFiltersBtn) {
        resetFiltersBtn.addEventListener('click', resetFilters);
    }

    // Sort functionality
    sortLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const sortBy = this.getAttribute('data-sort');
            sortChallenges(sortBy);
        });
    });

    // Bookmark functionality
    bookmarkBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            toggleBookmark(this);
        });
    });

    // Functions

    function filterChallenges() {
        const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
        const selectedDifficulties = getSelectedDifficulties();

        let visibleCount = 0;

        challengeItems.forEach(item => {
            const title = item.getAttribute('data-title');
            const difficulty = item.getAttribute('data-difficulty');

            const matchesSearch = title.includes(searchTerm);
            const matchesDifficulty = selectedDifficulties.includes(difficulty);

            if (matchesSearch && matchesDifficulty) {
                item.classList.remove('hidden');
                visibleCount++;
            } else {
                item.classList.add('hidden');
            }
        });

        updateEmptyState(visibleCount);
    }

    function getSelectedDifficulties() {
        const difficulties = [];
        if (document.getElementById('difficultyEasy')?.checked) difficulties.push('easy');
        if (document.getElementById('difficultyMedium')?.checked) difficulties.push('medium');
        if (document.getElementById('difficultyHard')?.checked) difficulties.push('hard');
        return difficulties;
    }

    function resetFilters() {
        // Reset search
        if (searchInput) searchInput.value = '';

        // Reset checkboxes
        document.querySelectorAll('.filter-options input[type="checkbox"]').forEach(checkbox => {
            checkbox.checked = true;
        });

        // Show all challenges
        challengeItems.forEach(item => {
            item.classList.remove('hidden');
        });

        updateEmptyState(challengeItems.length);
    }

    function sortChallenges(sortBy) {
        const grid = document.getElementById('challengesGrid');
        const items = Array.from(challengeItems);

        items.sort((a, b) => {
            switch (sortBy) {
                case 'difficulty':
                    const difficultyOrder = { 'easy': 1, 'medium': 2, 'hard': 3 };
                    const aDiff = difficultyOrder[a.getAttribute('data-difficulty')];
                    const bDiff = difficultyOrder[b.getAttribute('data-difficulty')];
                    return aDiff - bDiff;

                case 'title':
                    const aTitle = a.getAttribute('data-title');
                    const bTitle = b.getAttribute('data-title');
                    return aTitle.localeCompare(bTitle);

                case 'points':
                    const aPoints = parseInt(a.getAttribute('data-points'));
                    const bPoints = parseInt(b.getAttribute('data-points'));
                    return bPoints - aPoints;

                default:
                    return 0;
            }
        });

        // Reorder in DOM
        items.forEach(item => grid.appendChild(item));

        // Add animation
        items.forEach((item, index) => {
            item.style.animation = 'none';
            setTimeout(() => {
                item.style.animation = `slideIn 0.3s ease-out ${index * 0.05}s`;
            }, 10);
        });
    }

    function toggleBookmark(btn) {
        const challengeId = btn.getAttribute('data-id');
        const index = bookmarks.indexOf(challengeId);

        if (index > -1) {
            bookmarks.splice(index, 1);
            btn.classList.remove('active');
        } else {
            bookmarks.push(challengeId);
            btn.classList.add('active');
        }

        // Store in memory (in a real app, this would be sent to the server)
        console.log('Bookmarks:', bookmarks);
    }

    function initializeBookmarks() {
        // In a real app, load bookmarks from server
        // For now, just initialize empty
        bookmarks = [];
    }

    function updateEmptyState(visibleCount) {
        const grid = document.getElementById('challengesGrid');
        let emptyState = grid.querySelector('.empty-state');

        if (visibleCount === 0) {
            if (!emptyState) {
                emptyState = document.createElement('div');
                emptyState.className = 'col-12 empty-state text-center py-5';
                emptyState.innerHTML = `
                    <i class="bi bi-search" style="font-size: 4rem; color: #ccc;"></i>
                    <h4 class="mt-3">Нічого не знайдено</h4>
                    <p class="text-muted">Спробуйте змінити фільтри або пошуковий запит</p>
                `;
                grid.appendChild(emptyState);
            }
        } else {
            if (emptyState) {
                emptyState.remove();
            }
        }
    }

    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Hover effects for challenge cards
    challengeItems.forEach(item => {
        const card = item.querySelector('.challenge-card');
        if (card) {
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-5px)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });
        }
    });
});