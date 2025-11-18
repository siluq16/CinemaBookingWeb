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
                        html += `
                        <div class="col mb-4">
                            <div class="card card-movie">
                                <a href="/Film/Detail/${film.maPhim}" class="card card-movie">
                                    <img src="/images/poster/${film.poster}" alt="${film.tenPhim}" class="card-img-top movie-list-item-img" />
                                    <div class="card-body">
                                        <h5 class="card-title">${film.tenPhim}</h5>
                                        <span class="movie_genre">${film.theLoai ?? ''}</span>
                                        <h5 class="card-title">Ngày khởi chiếu: ${ngayFormat}</h5>

                                        <!-- Bỏ hoàn toàn nút Mua vé và phần đánh giá -->
                                    </div>
                                </a>
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
