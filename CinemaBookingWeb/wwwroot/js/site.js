const nav = document.querySelector(".nav-items")
const open = document.getElementById("open")
const close = document.getElementById("close")

open.addEventListener("click", () => {
    nav.style.display = "flex";
    nav.style.top = "0%"
})
close.addEventListener("click", () => {
    nav.style.top = "-110%"
    nav.style.display = "none";
})



window.addEventListener("scroll", () => {
    let bar = document.getElementById('header');
    if (window.scrollY > 50) {
        bar.style.background = 'rgba(20, 20, 20, 0.7)';
        bar.style.backdropFilter = 'blur(10px)';
    }
    else {
        bar.style.backdropFilter = ''
        bar.style.background = '';
    }
})

window.addEventListener('resize', function () {
    if (window.innerWidth > 1200) {
        document.querySelector('.nav-items').style.display = 'flex';
    }
    else {
        document.querySelector('.nav-items').style.display = 'none';
    }
});

window.onload = function () {
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
};


window.addEventListener("scroll", () => {
    if (window.scrollY > 50) {
        search.style.background = 'rgba(20, 20, 20, 0.7)';
        search.style.backdropFilter = 'blur(10px)';
    }
    else {
        search.style.backdropFilter = ''
        search.style.background = '';
    }
})
