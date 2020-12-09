CREATE DATABASE IF NOT EXISTS MyTestDB;

CREATE TABLE IF NOT EXISTS MyTestDB.Products (
    ProductId int not null,
    Price decimal not null,
    PRIMARY KEY(ProductId)
) ENGINE=InnoDB;