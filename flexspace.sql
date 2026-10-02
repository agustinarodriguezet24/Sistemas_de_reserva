-- phpMyAdmin SQL Dump
-- version 5.0.2
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 02-10-2026 a las 02:52:10
-- Versión del servidor: 10.4.14-MariaDB
-- Versión de PHP: 7.4.10

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `flexspace`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cliente`
--

CREATE TABLE `cliente` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TipoCliente` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `SancionesActivas` int(11) NOT NULL DEFAULT 0
) ;

--
-- Volcado de datos para la tabla `cliente`
--

INSERT INTO `cliente` (`Id`, `Nombre`, `Email`, `TipoCliente`, `SancionesActivas`) VALUES
(1, 'Ana Pérez', 'ana@mail.com', 'Estandar', 0),
(2, 'Bruno Gómez', 'bruno@mail.com', 'VIP', 0),
(3, 'Carla Díaz', 'carla@mail.com', 'Estandar', 1),
(4, 'Diego Ruiz', 'diego@mail.com', 'Estandar', 3);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `puesto`
--

CREATE TABLE `puesto` (
  `Id` int(11) NOT NULL,
  `Codigo` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TipoPuesto` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TarifaBasePorHora` decimal(10,2) NOT NULL
) ;

--
-- Volcado de datos para la tabla `puesto`
--

INSERT INTO `puesto` (`Id`, `Codigo`, `TipoPuesto`, `TarifaBasePorHora`) VALUES
(1, 'ESC-01', 'EscritorioIndividual', '1500.00'),
(2, 'SALA-01', 'SalaReuniones', '5000.00'),
(3, 'CAB-01', 'CabinaPrivada', '3000.00');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `reserva`
--

CREATE TABLE `reserva` (
  `Id` int(11) NOT NULL,
  `ClienteId` int(11) NOT NULL,
  `PuestoId` int(11) NOT NULL,
  `FechaInicio` datetime NOT NULL,
  `FechaFin` datetime NOT NULL,
  `Estado` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CostoTotal` decimal(10,2) NOT NULL
) ;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `cliente`
--
ALTER TABLE `cliente`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Email` (`Email`);

--
-- Indices de la tabla `puesto`
--
ALTER TABLE `puesto`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Codigo` (`Codigo`);

--
-- Indices de la tabla `reserva`
--
ALTER TABLE `reserva`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Reserva_Puesto` (`PuestoId`,`Estado`,`FechaInicio`,`FechaFin`),
  ADD KEY `IX_Reserva_Cliente` (`ClienteId`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `cliente`
--
ALTER TABLE `cliente`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `puesto`
--
ALTER TABLE `puesto`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `reserva`
--
ALTER TABLE `reserva`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `reserva`
--
ALTER TABLE `reserva`
  ADD CONSTRAINT `FK_Reserva_Cliente` FOREIGN KEY (`ClienteId`) REFERENCES `cliente` (`Id`),
  ADD CONSTRAINT `FK_Reserva_Puesto` FOREIGN KEY (`PuestoId`) REFERENCES `puesto` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
