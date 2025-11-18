function feedbackInfo() {
    const name = document.getElementById('name').value.trim();
    const email = document.getElementById('email').value.trim();
    const phone = document.getElementById('phone').value.trim();
    const message = document.getElementById('message').value.trim();

    if (!name || !email || !phone || !message) {
        alert('Vui lòng điền đầy đủ thông tin.');
        return;
    }

    fetch('/Contact/SendFeedback', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, phone, message })
    })
        .then(response => {
            if (!response.ok) throw new Error('Lỗi mạng hoặc server');
            return response.text();
        })
        .then(result => {
            alert('Cảm ơn bạn đã gửi phản hồi!');
            document.getElementById('feedbackForm').reset();
        })
        .catch(error => {
            alert('Gửi phản hồi thất bại. Vui lòng thử lại.');
            console.error(error);
        });
}
function resetForm() {
    document.getElementById('feedbackForm').reset(); // Reset tất cả các trường trong form
}

function hideAllSections() {
    const sections = document.querySelectorAll('.section-content');
    sections.forEach(section => section.classList.add('hidden'));
}

// Hàm để hủy kích hoạt tất cả các liên kết sidebar
function deactivateAllLinks() {
    const links = document.querySelectorAll('.sidebar a');
    links.forEach(link => link.classList.remove('active'));
}

function showSectionAndActivateLink(id) {
    hideAllSections();
    deactivateAllLinks();

    const sectionToShow = document.getElementById(id);
    if (sectionToShow) {
        sectionToShow.classList.remove('hidden');
        window.scrollTo(0, 0); // Đảm bảo trang luôn hiển thị từ đầu
    }

    const activeLink = document.getElementById('link-' + id);
    if (activeLink) {
        activeLink.classList.add('active');
    }
}

window.onload = function () {
    const hash = window.location.hash.substring(1);
    if (hash) {
        showSectionAndActivateLink(hash);
    } else {
        showSectionAndActivateLink('gioithieu');
    }
    const currentPage = window.location.pathname.split('/').pop().split('#')[0];
    const navLinks = document.querySelectorAll('.nav-items a');

    navLinks.forEach(link => {
        link.classList.remove('active');
    });

    navLinks.forEach(link => {
        const linkPage = link.getAttribute('href').split('/').pop().split('#')[0];
        if (linkPage === currentPage) {
            link.classList.add('active');
        }
    });
}

window.addEventListener('hashchange', function () {
    const newHash = window.location.hash.substring(1);
    showSectionAndActivateLink(newHash);
});