document.addEventListener("DOMContentLoaded", function () {
    const nowShowingLink = document.getElementById("now-showing");
    const upcomingLink = document.getElementById("upcoming-movies");
    const nowContent = document.getElementById("now-showing-content");
    const upContent = document.getElementById("upcoming-content");

    upcomingLink.addEventListener("click", function (e) {
        e.preventDefault();

        nowShowingLink.classList.remove("active");
        upcomingLink.classList.add("active");
        nowContent.classList.add("d-none");
        upContent.classList.remove("d-none");

        // Nếu chưa có dữ liệu -> gọi API
        if (upContent.innerHTML.trim() === "") {
            fetch("/Film/GetUpcomingMovies")
                .then(res => res.json())
                .then(data => {
                    console.log(data); // kiểm tra dữ liệu

                    let html = "";
                    data.forEach(film => {
                        const ngay = new Date(film.ngayKhoiChieu);
                        const ngayFormat = ngay.toLocaleDateString('vi-VN');
                        let ageRatingText = "P";
                        let ageDataAttr = "p";

                        if (film.doTuoi && film.doTuoi > 0) {
                            ageRatingText = film.doTuoi + "+";
                            ageDataAttr = ageRatingText.toLowerCase(); 
                        }
                        html += `
                        <div class="col mb-4">
                            <div class="card card-movie">
                                <a href="/Film/Detail/${film.maPhim}" class="movie-link-wrapper">
                                    <div class="movie-age" data-age="${ageDataAttr}">
                                        ${ageRatingText}
                                    </div>
                                    <img src="/images/poster/${film.poster}" alt="${film.tenPhim}" class="card-img-top movie-list-item-img" />
                                </a>

                                <div class="card-body">
                                    <div class="movie-info">
                                        <a href="/Film/Detail/${film.maPhim}" class="text-decoration-none">
                                            <h5 class="card-title">${film.tenPhim}</h5>
                                        </a>
                    
                                        <span class="movie_genre">${film.theLoai ?? ''}</span>
                    
                                        <div class="release-date mt-2">
                                            <i class="fas fa-calendar-alt me-1"></i> Khởi chiếu: 
                                            <span class="text-white fw-bold">${ngayFormat}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                    });

                    upContent.innerHTML = html;
                });
        }
    });

    nowShowingLink.addEventListener("click", function (e) {
        e.preventDefault();
        nowShowingLink.classList.add("active");
        upcomingLink.classList.remove("active");
        nowContent.classList.remove("d-none");
        upContent.classList.add("d-none");
    });
});
