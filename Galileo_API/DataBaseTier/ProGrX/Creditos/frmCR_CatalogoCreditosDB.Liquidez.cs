using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

        /**
         * Obtiene los rangos por liquidez de la linea.
         * @param codEmpresa Codigo de empresa.
         * @param codigo Codigo de linea de credito.
         */
        public ErrorDto<CrCatalogoCreditoRangosLiquidezData> CrCatalogoCreditos_RangosLiquidez_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ErrorRangosLiquidez(ensure.Description);
            }

            const string bonoQuery = @"
                SELECT id,
                    pago_inicial,
                    pago_final,
                    puntos_bonificacion
                FROM CRD_CATALOGO_LIQUIDEZ_BONO
                WHERE codigo = @Codigo
                ORDER BY id;";

            const string capacidadQuery = @"
                SELECT id,
                    capacidad_inicio,
                    capacidad_corte,
                    porc_giro_maximo,
                    porcentaje_olgura
                FROM CRD_CATALOGO_LIQUIDEZ_CAPACIDAD
                WHERE codigo = @Codigo
                ORDER BY id;";

            var parametros = new { Codigo = codigo };
            var bono = DbHelper.ExecuteListQuery<CrCatalogoCreditoLiquidezBonoData>(_portalDb, codEmpresa, bonoQuery, parametros);
            if (bono.Code < 0) return ErrorRangosLiquidez(bono.Description);

            var capacidad = DbHelper.ExecuteListQuery<CrCatalogoCreditoLiquidezCapacidadData>(_portalDb, codEmpresa, capacidadQuery, parametros);
            if (capacidad.Code < 0) return ErrorRangosLiquidez(capacidad.Description);

            return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoRangosLiquidezData
                {
                    bono = bono.Result ?? [],
                    capacidad = capacidad.Result ?? []
                }
            };
        }


        /**
         * Guarda un rango por liquidez de bono.
         * @param codEmpresa Codigo de empresa.
         * @param request Datos del rango.
         */
        public ErrorDto CrCatalogoCreditos_LiquidezBono_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            NormalizarLiquidezBonoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || request.rango.id <= 0)
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el rango." };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ensure;
            }

            const string query = @"
                IF EXISTS (SELECT 1 FROM CRD_CATALOGO_LIQUIDEZ_BONO WHERE codigo = @Codigo AND id = @Id)
                BEGIN
                    UPDATE CRD_CATALOGO_LIQUIDEZ_BONO
                    SET pago_inicial = @PagoInicial,
                        pago_final = @PagoFinal,
                        puntos_bonificacion = @PuntosBonificacion,
                        modifica_fecha = dbo.MyGetdate(),
                        modifica_usuario = @Usuario
                    WHERE codigo = @Codigo
                        AND id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO CRD_CATALOGO_LIQUIDEZ_BONO(
                        codigo, id, pago_inicial, pago_final, puntos_bonificacion, registro_fecha, registro_usuario)
                    VALUES(
                        @Codigo, @Id, @PagoInicial, @PagoFinal, @PuntosBonificacion, dbo.MyGetdate(), @Usuario);
                END";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Id = request.rango.id,
                    PagoInicial = request.rango.pago_inicial,
                    PagoFinal = request.rango.pago_final,
                    PuntosBonificacion = request.rango.puntos_bonificacion,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Rango Liquidez Bono Linea: {request.codigo} ID:{request.rango.id}");
            }

            return respuesta;
        }


        /**
         * Guarda un rango por liquidez de capacidad de pago.
         * @param codEmpresa Codigo de empresa.
         * @param request Datos del rango.
         */
        public ErrorDto CrCatalogoCreditos_LiquidezCapacidad_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            NormalizarLiquidezCapacidadRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || request.rango.id <= 0)
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el rango." };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ensure;
            }

            const string query = @"
                IF EXISTS (SELECT 1 FROM CRD_CATALOGO_LIQUIDEZ_CAPACIDAD WHERE codigo = @Codigo AND id = @Id)
                BEGIN
                    UPDATE CRD_CATALOGO_LIQUIDEZ_CAPACIDAD
                    SET capacidad_inicio = @CapacidadInicio,
                        capacidad_corte = @CapacidadCorte,
                        porc_giro_maximo = @PorcGiroMaximo,
                        porcentaje_olgura = @PorcentajeOlgura,
                        modifica_fecha = dbo.MyGetdate(),
                        modifica_usuario = @Usuario
                    WHERE codigo = @Codigo
                        AND id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO CRD_CATALOGO_LIQUIDEZ_CAPACIDAD(
                        codigo, id, capacidad_inicio, capacidad_corte, porc_giro_maximo, porcentaje_olgura, registro_fecha, registro_usuario)
                    VALUES(
                        @Codigo, @Id, @CapacidadInicio, @CapacidadCorte, @PorcGiroMaximo, @PorcentajeOlgura, dbo.MyGetdate(), @Usuario);
                END";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Id = request.rango.id,
                    CapacidadInicio = request.rango.capacidad_inicio,
                    CapacidadCorte = request.rango.capacidad_corte,
                    PorcGiroMaximo = request.rango.porc_giro_maximo,
                    PorcentajeOlgura = request.rango.porcentaje_olgura,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Rango Liquidez Capacidad Linea: {request.codigo} ID:{request.rango.id}");
            }

            return respuesta;
        }


        private static ErrorDto<CrCatalogoCreditoRangosLiquidezData> ErrorRangosLiquidez(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener rangos por liquidez de la linea."
            };
        }


        private ErrorDto AsegurarTablasRangosLiquidez(int codEmpresa)
        {
            const string query = @"
                IF OBJECT_ID('dbo.CRD_CATALOGO_LIQUIDEZ_BONO', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.CRD_CATALOGO_LIQUIDEZ_BONO(
                        codigo varchar(4) NOT NULL,
                        id int NOT NULL,
                        pago_inicial decimal(18, 4) NULL,
                        pago_final decimal(18, 4) NULL,
                        puntos_bonificacion decimal(18, 4) NULL,
                        registro_fecha datetime NULL,
                        registro_usuario varchar(50) NULL,
                        modifica_fecha datetime NULL,
                        modifica_usuario varchar(50) NULL,
                        CONSTRAINT PK_CRD_CATALOGO_LIQUIDEZ_BONO PRIMARY KEY(codigo, id)
                    );
                END

                IF OBJECT_ID('dbo.CRD_CATALOGO_LIQUIDEZ_CAPACIDAD', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.CRD_CATALOGO_LIQUIDEZ_CAPACIDAD(
                        codigo varchar(4) NOT NULL,
                        id int NOT NULL,
                        capacidad_inicio decimal(18, 4) NULL,
                        capacidad_corte decimal(18, 4) NULL,
                        porc_giro_maximo decimal(18, 4) NULL,
                        porcentaje_olgura decimal(18, 4) NULL,
                        registro_fecha datetime NULL,
                        registro_usuario varchar(50) NULL,
                        modifica_fecha datetime NULL,
                        modifica_usuario varchar(50) NULL,
                        CONSTRAINT PK_CRD_CATALOGO_LIQUIDEZ_CAPACIDAD PRIMARY KEY(codigo, id)
                    );
                END";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { });
        }
    }
}
