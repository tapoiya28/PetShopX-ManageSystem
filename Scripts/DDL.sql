USE master;
GO

IF EXISTS (SELECT 1 FROM SYS.databases WHERE name = 'PetcareX')
    DROP DATABASE PetcareX;
go

CREATE DATABASE PetcareX;
GO
USE PetcareX;
GO

--- KHACH HANG VA THU CUNG

--- KHAM BENH VA TIEM PHONG
create table catiem (
    matiem integer IDENTITY(1,1),
    matc integer,
    ngaytiem date,
    manv varchar(10),
    constraint pk_matiem primary key (matiem)
)
create table chitietcatiem (
    matiem integer,
    mavacxin char(5) check (mavacxin like 'VC%'),
    soluong integer check (soluong > 0),
    constraint pk_chitietcatiem primary key(matiem,mavacxin)
)
create table cakhambenh (
    makb integer IDENTITY(1,1),
    matc integer,
    manv integer,
    ngaykham date,
    constraint pk_cakhambenh primary key (makb)
)
create table trieuchung (
    makb integer,
    tentrieuchung nvarchar (50),
    constraint pk_trieuchung primary key (makb, tentrieuchung)
)
create table chandoan (
    makb integer,
    tenchandoan nvarchar (20),
    constraint pk_chandoan primary key(makb, tenchandoan)
)
create table toathuoc (
    matt integer IDENTITY(1,1),
    makb integer,
    ghichu nvarchar(50),
    constraint pk_toathuoc primary key (matt)
)
create table chitiettoathuoc (
    matt integer,
    mathuoc char(5) check (mathuoc like 'TH%'),
    soluong integer check(soluong > 0),
    constraint pk_chitiettoathuoc primary key (matt,mathuoc)
)

--- KINH DOANH

--- CHI NHANH VA NHAN SU

------
-- KHOA NGOAI
------
alter table catiem 
add 
constraint fk_catiem_nhanvien FOREIGN KEY(manv) REFERENCES nhanvien,
constraint fk_catiem_thucung FOREIGN KEY (matc) REFERENCES thucung;
go
alter table cakhambenh
ADD
constraint fk_cakhambenh_nhanvien FOREIGN KEY(manv) REFERENCES nhanvien,
constraint fk_cakhambenh_thucung FOREIGN KEY(matc) REFERENCES thucung;
go
alter table chitietcatiem 
ADD
constraint fk_chitietcatiem_catiem FOREIGN KEY (matiem) REFERENCES catiem,
constraint fk_chitietcatiem_sanpham foreign key (mavacxin) REFERENCES sanpham;
GO
alter table trieuchung 
ADD
constraint fk_trieuchung_cakhambenh foreign key (makb) REFERENCES cakhambenh;
GO
alter table chandoan 
ADD
constraint fk_chandoan_cakhambenh FOREIGN KEY (makb) REFERENCES cakhambenh;
GO
alter table toathuoc
ADD
constraint fk_toathuoc_cakhambenh FOREIGN KEY (makb) REFERENCES cakhambenh;
GO
alter table chitiettoathuoc
ADD
constraint fk_chitiettoathuoc_cakhambenh FOREIGN KEY (makb) REFERENCES cakhambenh,
constraint fk_chitiettoathuoc_sanpham FOREIGN KEY (mathuoc) REFERENCES sanpham;