# ==============================================================================
# Giai đoạn 1: BUILD - Sử dụng .NET SDK để build và publish ứng dụng.
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép và restore các file project để tận dụng Docker Cache
# (Giúp build nhanh hơn nếu chỉ thay đổi mã nguồn, không thay đổi dependencies)
COPY ["CinemaBookingWeb/CinemaBookingWeb.csproj", "CinemaBookingWeb/"]
COPY ["CinemaBookingWeb.DataAccess/CinemaBookingWeb.DataAccess.csproj", "CinemaBookingWeb.DataAccess/"]
COPY ["CinemaBookingWeb.Models/CinemaBookingWeb.Models.csproj", "CinemaBookingWeb.Models/"]
COPY ["CinemaBookingWeb.Utility/CinemaBookingWeb.Utility.csproj", "CinemaBookingWeb.Utility/"]

# Thực hiện restore dependencies
RUN dotnet restore "CinemaBookingWeb/CinemaBookingWeb.csproj"

# Sao chép toàn bộ source code
COPY . .
WORKDIR /src/CinemaBookingWeb

# Thực hiện publish (build final output)
# Tên thư mục output là /app/publish
RUN dotnet publish "CinemaBookingWeb.csproj" -c Release -o /app/publish

# ==============================================================================
# Giai đoạn 2: FINAL - Sử dụng .NET Runtime nhỏ gọn để chạy ứng dụng
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy output đã publish từ giai đoạn 'build'
COPY --from=build /app/publish .

# Định nghĩa cổng mà ứng dụng lắng nghe.
# Render sẽ tự động ánh xạ cổng công cộng tới cổng này.
# Sử dụng 8080 theo khuyến nghị của các nền tảng hosting hiện đại.
EXPOSE 8080

# Chạy ứng dụng web
# Đảm bảo tên file DLL khớp với tên file DLL đã được publish
ENTRYPOINT ["dotnet", "CinemaBookingWeb.dll"]