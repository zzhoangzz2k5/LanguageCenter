-- Clean up products first to avoid FK constraint issues
DELETE FROM Products;
DELETE FROM Categories;

-- Reseed identities to 0 so they both start at 1 on next insert
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);

-- Insert Categories
INSERT INTO Categories (Name, Description, Photo) VALUES 
(N'iPhone', N'Apple iOS Smartphones', 'cat-1-1.png'),
(N'Samsung', N'Samsung Android Smartphones', 'cat-1-2.png'),
(N'Xiaomi', N'Xiaomi Mobile Devices', 'cat-1-3.png'),
(N'Oppo', N'Oppo Mobile Devices', 'cat-1-4.png'),
(N'Accessories', N'Mobile Accessories and Cables', 'cat-1-5.png');

-- Insert Products (matching CategoryIds: 1 = iPhone, 2 = Samsung, 3 = Xiaomi, 4 = Oppo, 5 = Accessories)
INSERT INTO Products (Name, Slug, Description, Price, PriceSale, Photo, CategoryId, Featured) VALUES
(N'iPhone 14 Pro Max', 'iphone-14-pro-max', N'Apple iPhone 14 Pro Max 256GB - Deep Purple. Features dynamic island, A16 Bionic chip, and 48MP camera.', 1099.00, 999.00, 'product-1-1.png', 1, 1),
(N'iPhone 14', 'iphone-14', N'Apple iPhone 14 128GB - Midnight. Super Retina XDR display, A15 Bionic chip, advanced dual-camera system.', 799.00, 749.00, 'product-1-3.png', 1, 1),
(N'Samsung Galaxy S23 Ultra', 'samsung-galaxy-s23-ultra', N'Samsung Galaxy S23 Ultra 5G 256GB - Phantom Black. 200MP camera, built-in S Pen, Snapdragon 8 Gen 2.', 1199.00, 1099.00, 'product-2-1.png', 2, 1),
(N'Samsung Galaxy S23', 'samsung-galaxy-s23', N'Samsung Galaxy S23 5G 128GB - Cream. 50MP camera, Snapdragon 8 Gen 2 processor, intelligent battery.', 899.00, 799.00, 'product-2-2.png', 2, 1),
(N'Xiaomi Mi 11 Ultra', 'xiaomi-mi-11-ultra', N'Xiaomi Mi 11 Ultra 12GB+256GB - Ceramic White. Dual-pixel pro-grade cameras, ceramic body design.', 590.00, 590.00, 'product-3-1.png', 3, 0),
(N'Xiaomi Mi 11', 'xiaomi-mi-11', N'Xiaomi Mi 11 5G 8GB+128GB - Horizon Blue. Snapdragon 888 processor, 108MP camera, 120Hz AMOLED display.', 499.00, 459.00, 'product-3-2.png', 3, 0),
(N'Oppo Find X5 Pro', 'oppo-find-x5-pro', N'Oppo Find X5 Pro 5G 256GB - Ceramic Black. Hasselblad camera for mobile, MariSilicon X NPU.', 899.00, 799.00, 'product-4-1.png', 4, 0),
(N'Oppo Find X5', 'oppo-find-x5', N'Oppo Find X5 5G 128GB - Black. 4K Ultra Night Video, MariSilicon X, Hasselblad Camera for Mobile.', 699.00, 599.00, 'product-4-2.png', 4, 0),
(N'Smart Headphones', 'smart-headphones', N'Premium Noise Cancelling Wireless Over-Ear Headphones with high fidelity audio and long battery life.', 199.00, 149.00, 'product-1-2.png', 5, 0),
(N'Smart Watch Series 8', 'smart-watch-series-8', N'Advanced fitness tracker, blood oxygen sensor, and heart rate monitor. Stay connected on the go.', 399.00, 349.00, 'product-2-2.png', 5, 0),
(N'High-Speed HDMI Cable', 'high-speed-hdmi-cable', N'Basics High-Speed HDMI Cable 18 Gbps, 4K/60Hz, Ethernet, 3D, and Audio Return Channel.', 20.00, 15.00, 'product-2-3.png', 5, 0),
(N'Laptops Core i7', 'laptops-core-i7', N'15.6 inch FHD IPS Screen, Intel Core i7 Processor, 16GB RAM, 512GB SSD Windows 11 Laptop.', 999.00, 899.00, 'product-1-5.png', 5, 0);
