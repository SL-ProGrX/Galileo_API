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
        /// Consulta plantilla asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="codPlantilla">Código de la plantilla.</param>
        /// <returns></returns>
        public ErrorDto<CntxPlantillaResponseDto> Consultar(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                var header = cn.QueryFirstOrDefault<CntxPlantillaDto>(
                    @"SELECT *
                      FROM CntX_Plantilla_Asientos
                      WHERE cod_contabilidad = @codContabilidad
                        AND cod_plantilla = @codPlantilla",
                    new { codContabilidad, codPlantilla });

                var detalle = cn.Query<CntxPlantillaDetalleDto>(
                    @"SELECT *
                      FROM CntX_Plantilla_detalle
                      WHERE cod_contabilidad = @codContabilidad
                        AND cod_plantilla = @codPlantilla
                      ORDER BY num_linea",
                    new { codContabilidad, codPlantilla }).ToList();

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
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                using var transaction = cn.BeginTransaction();
                if (modelo.header.cod_contabilidad is null)
                    throw new InvalidOperationException("La contabilidad es requerida.");
                ValidarModelo(cn, transaction, modelo, validarPeriodo: true);

                var nuevoCodigo = cn.QueryFirst<int>(
                    @"SELECT ISNULL(MAX(cod_plantilla),0) + 1
                      FROM CntX_Plantilla_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad",
                    new { modelo.header.cod_contabilidad },
                    transaction);

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
                    modelo.header,
                    transaction);

                foreach (var d in modelo.detalle)
                {
                    d.cod_plantilla = nuevoCodigo;
                    d.cod_contabilidad = modelo.header.cod_contabilidad;
                    d.num_linea = modelo.detalle.IndexOf(d) + 1;

                    cn.Execute(
                        @"INSERT INTO CntX_Plantilla_detalle
                        (cod_plantilla,cod_contabilidad,num_linea,
                         cod_cuenta,cod_unidad,cod_centro_costo,
                         cod_divisa,tc,inc_tipo,inc_valor,debitos,creditos)
                        VALUES
                        (@cod_plantilla,@cod_contabilidad,@num_linea,
                         @cod_cuenta,@cod_unidad,@cod_centro_costo,
                         @cod_divisa,@tc,@inc_tipo,@inc_valor,@debitos,@creditos)",
                        d,
                        transaction);
                }

                transaction.Commit();
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
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                using var transaction = cn.BeginTransaction();
                if (modelo.header.cod_contabilidad is null)
                    throw new InvalidOperationException("La contabilidad es requerida.");
                ValidarModelo(cn, transaction, modelo, validarPeriodo: false);

                cn.Execute(
                    @"UPDATE CntX_Plantilla_Asientos
                      SET descripcion=@descripcion,
                          tipo_asiento=@tipo_asiento,
                          anio_inicio=@anio_inicio,
                          mes_inicio=@mes_inicio,
                          asiento_descripcion=@asiento_descripcion,
                          asiento_detalle=@asiento_detalle,
                          asiento_documento=@asiento_documento
                      WHERE cod_contabilidad=@cod_contabilidad
                        AND cod_plantilla=@cod_plantilla",
                    modelo.header,
                    transaction);

                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_detalle
                      WHERE cod_contabilidad=@cod_contabilidad
                        AND cod_plantilla=@cod_plantilla",
                    modelo.header,
                    transaction);

                for (var index = 0; index < modelo.detalle.Count; index++)
                {
                    var d = modelo.detalle[index];
                    d.cod_plantilla = modelo.header.cod_plantilla;
                    d.cod_contabilidad = modelo.header.cod_contabilidad;
                    d.num_linea = index + 1;
                    cn.Execute(
                        @"INSERT INTO CntX_Plantilla_detalle
                        (cod_plantilla,cod_contabilidad,num_linea,
                         cod_cuenta,cod_unidad,cod_centro_costo,
                         cod_divisa,tc,inc_tipo,inc_valor,debitos,creditos)
                        VALUES
                        (@cod_plantilla,@cod_contabilidad,@num_linea,
                         @cod_cuenta,@cod_unidad,@cod_centro_costo,
                         @cod_divisa,@tc,@inc_tipo,@inc_valor,@debitos,@creditos)",
                        d,
                        transaction);
                }

                transaction.Commit();
                return modelo.header.cod_plantilla;
            });
        }


        /// <summary>
        /// Borra los registros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="codPlantilla">Código de la plantilla.</param>
        /// <returns></returns>
        public ErrorDto<int> Borrar(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                using var transaction = cn.BeginTransaction();
                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_detalle
                      WHERE cod_contabilidad=@codContabilidad
                        AND cod_plantilla=@codPlantilla",
                    new { codContabilidad, codPlantilla },
                    transaction);

                cn.Execute(
                    @"DELETE FROM CntX_Plantilla_Asientos
                      WHERE cod_contabilidad=@codContabilidad
                        AND cod_plantilla=@codPlantilla",
                    new { codContabilidad, codPlantilla },
                    transaction);

                transaction.Commit();
                return 1;
            });
        }


        /// <summary>
        /// Realiza funcion del scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="codigoActual"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<int?> Scroll(int codEmpresa, int codContabilidad, int? codigoActual, int direccion)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                string sql = direccion > 0
                    ? @"SELECT TOP 1 cod_plantilla
                        FROM CntX_Plantilla_Asientos
                        WHERE cod_contabilidad = @codContabilidad
                          AND cod_plantilla > @codigoActual
                        ORDER BY cod_plantilla"
                    : @"SELECT TOP 1 cod_plantilla
                        FROM CntX_Plantilla_Asientos
                        WHERE cod_contabilidad = @codContabilidad
                          AND cod_plantilla < @codigoActual
                        ORDER BY cod_plantilla DESC";

                return cn.QueryFirstOrDefault<int?>(
                    sql,
                    new { codContabilidad, codigoActual = codigoActual ?? 0 });
            });
        }

        /// <summary>
        /// Busca las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns></returns>
        public ErrorDto<List<CntxPlantillaDto>> BuscarPlantillas(int codEmpresa, int codContabilidad)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                return cn.Query<CntxPlantillaDto>(
                    @"SELECT cod_plantilla, descripcion
                      FROM CntX_Plantilla_Asientos
                      WHERE cod_contabilidad = @codContabilidad
                      ORDER BY cod_plantilla",
                    new { codContabilidad }
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
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
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



        /// <summary>Obtiene las unidades de la contabilidad.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <returns>Lista de unidades disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
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


        /// <summary>Obtiene las divisas de la contabilidad.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <returns>Lista de divisas disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
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


        /// <summary>Obtiene los centros de costo permitidos para una unidad.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <param name="codUnidad">Código de unidad.</param>
        /// <returns>Lista de centros de costo disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, int cod_contabilidad, string codUnidad)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
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

        /// <summary>
        /// Valida el encabezado y detalle de una plantilla antes de persistirlos.
        /// </summary>
        /// <param name="cn">Conexión abierta a la empresa.</param>
        /// <param name="transaction">Transacción activa de la operación.</param>
        /// <param name="modelo">Plantilla que se desea guardar.</param>
        /// <param name="validarPeriodo">Indica si debe comprobarse que el periodo esté abierto.</param>
        private static void ValidarModelo(
            SqlConnection cn,
            SqlTransaction transaction,
            CntxPlantillaSaveDto modelo,
            bool validarPeriodo)
        {
            var header = modelo.header;
            if (string.IsNullOrWhiteSpace(header.descripcion))
                throw new InvalidOperationException("Debe indicar la descripción de la plantilla.");
            if (string.IsNullOrWhiteSpace(header.tipo_asiento))
                throw new InvalidOperationException("Debe indicar el tipo de asiento.");
            if (header.mes_inicio is < 1 or > 12 || header.anio_inicio is null)
                throw new InvalidOperationException("El periodo inicial no es válido.");
            if (modelo.detalle.Count == 0)
                throw new InvalidOperationException("Debe agregar al menos una línea de detalle.");

            if (validarPeriodo)
            {
                var periodoAbierto = cn.ExecuteScalar<int>(
                    @"SELECT COUNT(1)
                      FROM CntX_Periodos
                      WHERE cod_contabilidad = @cod_contabilidad
                        AND anio = @anio_inicio
                        AND mes = @mes_inicio
                        AND estado = 'P'",
                    header,
                    transaction);
                if (periodoAbierto == 0)
                    throw new InvalidOperationException("El periodo indicado está cerrado o no existe.");
            }

            var tipoValido = cn.ExecuteScalar<int>(
                @"SELECT COUNT(1)
                  FROM CntX_Tipos_Asientos
                  WHERE cod_contabilidad = @cod_contabilidad
                    AND tipo_asiento = @tipo_asiento",
                header,
                transaction);
            if (tipoValido == 0)
                throw new InvalidOperationException("El tipo de asiento indicado no existe.");

            var totalDebitos = modelo.detalle.Sum(x => x.debitos ?? 0);
            var totalCreditos = modelo.detalle.Sum(x => x.creditos ?? 0);
            if (totalDebitos != totalCreditos)
                throw new InvalidOperationException("El asiento no se encuentra balanceado.");

            foreach (var detalle in modelo.detalle)
            {
                if ((detalle.debitos ?? 0) > 0 && (detalle.creditos ?? 0) > 0)
                    throw new InvalidOperationException("Una línea no puede tener débito y crédito al mismo tiempo.");

                var cuentaValida = cn.ExecuteScalar<int>(
                    @"SELECT COUNT(1)
                      FROM CntX_Cuentas
                      WHERE cod_contabilidad = @cod_contabilidad
                        AND cod_cuenta = @cod_cuenta
                        AND acepta_movimientos = 1",
                    new
                    {
                        header.cod_contabilidad,
                        detalle.cod_cuenta
                    },
                    transaction);
                if (cuentaValida == 0)
                    throw new InvalidOperationException($"La cuenta {detalle.cod_cuenta} no existe o no acepta movimientos.");
            }
        }

    }
}
