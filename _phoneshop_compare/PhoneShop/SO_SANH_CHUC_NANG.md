# So sanh Hutech-PhoneShop va PhoneShop

## Chuc nang Hutech co, PhoneShop ban dau chua co

- Slug cho san pham.
- URL chi tiet than thien theo dang `/Product/{slug}`.
- Migration them cot `Slug`.
- Danh sach san pham noi bat lay theo truong `Featured`.

## Chuc nang PhoneShop da co nhieu hon Hutech

- Gio hang day du: them, xoa, cap nhat so luong.
- Trang gio hang va thanh toan.
- Quan ly danh muc trong khu vuc Admin.
- Cac model don hang, nguoi dung va vai tro.
- Du lieu mau cho danh muc va san pham.

## Noi dung da bo sung va sua

- Them `Slug` vao model, migration va du lieu mau.
- Tu dong tao slug duy nhat khi Admin tao san pham.
- Them route chi tiet san pham theo slug.
- Chuyen cac lien ket san pham tren trang chu sang URL slug.
- Loc dung san pham co `Featured = true`, co fallback khi chua co du lieu.
- Sua controller Admin Product ve dung thu muc va namespace.
- Sua form tao san pham, upload anh vao `wwwroot/img/products`.
- Them kiem tra gia sale va dinh dang file anh.
- Them trang thong bao dat hang thanh cong.
- Loai bo warning nullable cua `CartItem`.

## Lenh khoi tao

```powershell
dotnet restore
dotnet ef database update
dotnet run
```
