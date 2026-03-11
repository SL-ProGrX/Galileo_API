using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaAsientosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXPlantillaAsientosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Helper de Ejecutar
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        private ErrorDto<T> Ejecutar<T>(int codEmpresa, Func<SqlConnection, T> accion)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = accion(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Consulta plantilla asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<CntxPlantillaResponseDto> Consultar(int codEmpresa, int codPlantilla)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                var header = cn.QueryFirstOrDefault<CntxPlantillaDto>(
                    @"SELECT *
                      FROM CntX_Plantilla_Asientos
                      WHERE cod_plantilla = @codPlantilla",
                    new { codPlantilla });

                var detalle = cn.Query<CntxPlantillaDetalleDto>(
                    @"SELECT *
                      FROM CntX_Plantilla_detalle
                      WHERE cod_plantilla = @codPlantilla
                      ORDER BY num_linea",
                    new { codPlantilla }).ToList();

                return new CntxPlantillaResponseDto
                {
                    header = header,
                    detalle = detalle
                };
            });
        }


        /// <summary>
        /// Inserta nuevas plantillas asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="modelo"></param>
        /// <returns></returns>
        public ErrorDto<int> Insertar(int codEmpresa, CntxPlantillaSaveDto modelo)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                var nuevoCodigo = cn.QueryFirst<int>(
                    @"SELECT ISNULL(MAX(cod_plantilla),0) + 1
                      FROM CntX_Plantilla_Asientos");

                modelo.header.cod_plantilla = nuevoCodigo;

                cn.Execute(
                    @"INSERT INTO CntX_Plantilla_Asientos
                    (cod_plantilla,cod_contabilidad,descripcion,
                     tipo_asiento,anio_inicio,mes_inicio,
                     asiento_descripcion,asiento_detalle,asiento_documento)
                    VALUES
                    (@cod_plantilla,@cod_contabilidad,@descripcion,
                     @tipo_asiento,@anio_inicio,@mes_inicio,
                     @asiento_descripcion,@asiento_detalle,@asiento_documento)",
                    modelo.header);

                foreach (var d in modelo.detalle)
                {
                    d.cod_plantilla = nuevoCodigo;

                    cn.Execute(
                        @"INSERT INTO CntX_Plantilla_detalle
                        (cod_plantilla,cod_contabilidad,num_linea,
                         cod_cuenta,cod_unidad,cod_centro_costo,
                         cod_divisa,tc,inc_tipo,inc_valor,debitos,creditos)
                        VALUES
                        (@cod_plantilla,@cod_contabilidad,@num_linea,
                         @cod_cuenta,@cod_unidad,@cod_centro_costo,
                         @cod_divisa,@tc,@inc_tipo,@inc_valor,@debitos,@creditos)",
                        d);
                }

                return nuevoCodigo;
            });
        }


        /// <summary>
        /// Actualiza las plantillas asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="modelo"></param>
        /// <returns></returns>
        public ErrorDto<int?> Actualizar(int codEmpresa, CntxPlantillaSaveDto modelo)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                cn.Execute(
                    @"UPDATE CntX_Plantilla_Asientos
                      SET descripcion=@descripcion,
                          tipo_asiento=@tipo_asiento,
                          anio_inicio=@anio_inicio,
                          mes_inicio=@mes_inicio,
                          asiento_descripcion=@asiento_descripcion,
                          asiento_detalle=@asiento_detalle,
                          asiento_documento=@asiento_documento
                      WHERE cod_plantilla=@cod_plantilla",
                    modelo.header);

                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_detalle
                      WHERE cod_plantilla=@cod_plantilla",
                    modelo.header);

                foreach (var d in modelo.detalle)
                {
                    cn.Execute(
                        @"INSERT INTO CntX_Plantilla_detalle
                        (cod_plantilla,cod_contabilidad,num_linea,
                         cod_cuenta,cod_unidad,cod_centro_costo,
                         cod_divisa,tc,inc_tipo,inc_valor,debitos,creditos)
                        VALUES
                        (@cod_plantilla,@cod_contabilidad,@num_linea,
                         @cod_cuenta,@cod_unidad,@cod_centro_costo,
                         @cod_divisa,@tc,@inc_tipo,@inc_valor,@debitos,@creditos)",
                        d);
                }

                return modelo.header.cod_plantilla;
            });
        }


        /// <summary>
        /// Borra los registros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<int> Borrar(int codEmpresa, int codPlantilla)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_detalle
                      WHERE cod_plantilla=@codPlantilla",
                    new { codPlantilla });

                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_Asientos
                      WHERE cod_plantilla=@codPlantilla",
                    new { codPlantilla });

                return 1;
            });
        }


        /// <summary>
        /// Realiza funcion del scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigoActual"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                string sql = direccion > 0
                    ? @"SELECT TOP 1 cod_plantilla
                        FROM CntX_Plantilla_Asientos
                        WHERE cod_plantilla > @codigoActual
                        ORDER BY cod_plantilla"
                    : @"SELECT TOP 1 cod_plantilla
                        FROM CntX_Plantilla_Asientos
                        WHERE cod_plantilla < @codigoActual
                        ORDER BY cod_plantilla DESC";

                return cn.QueryFirstOrDefault<int?>(
                    sql,
                    new { codigoActual = codigoActual ?? 0 });
            });
        }

        /// <summary>
        /// Busca las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPlantillaDto>> BuscarPlantillas(int codEmpresa)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<CntxPlantillaDto>(
                    @"SELECT cod_plantilla, descripcion
                      FROM CntX_Plantilla_Asientos
                      ORDER BY cod_plantilla"
                ).ToList();
            });
        }

        /// <summary>
        /// Busca los tipo de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        tipo_asiento AS item,
                        descripcion
                      FROM CntX_Tipos_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY tipo_asiento",
                    new { cod_contabilidad }
                ).ToList();
            });
        }



        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                cod_unidad AS item,
                descripcion
              FROM CntX_Unidades
              WHERE cod_contabilidad = @cod_contabilidad
              ORDER BY cod_unidad",
                    new { cod_contabilidad }
                ).ToList();
            });
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                cod_divisa AS item,
                descripcion
              FROM CntX_Divisas
              WHERE cod_contabilidad = @cod_contabilidad
              ORDER BY cod_divisa",
                    new { cod_contabilidad }
                ).ToList();
            });
        }


        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, int cod_contabilidad, string codUnidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                C.cod_centro_costo AS item,
                C.descripcion
              FROM CntX_Centro_Costos C
              INNER JOIN CntX_Unidades_CC U
                ON C.cod_centro_costo = U.cod_centro_costo
                AND C.cod_contabilidad = U.cod_contabilidad
              WHERE C.cod_contabilidad = @cod_contabilidad
                AND U.cod_unidad = @codUnidad
              ORDER BY C.cod_centro_costo",
                    new { cod_contabilidad, codUnidad }
                ).ToList();
            });
        }

    }
}