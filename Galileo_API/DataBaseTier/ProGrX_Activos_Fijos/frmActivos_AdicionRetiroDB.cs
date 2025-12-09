using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosAdicionRetiroDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosAdicionRetiroDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mActivos        = new MActivosFijos(config);
            _portalDB        = new PortalDB(config);
        }

        #region Helpers privados

        private void RegistrarBitacora(
            int CodEmpresa,
            string usuario,
            string detalle,
            string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId         = CodEmpresa,
                Usuario           = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento        = movimiento,
                Modulo            = vModulo
            });
        }

        #endregion

        /// <summary>
        /// Método para consultar lista de justificaciones
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Justificaciones_Obtener(
            int CodEmpresa,
            string tipo)
        {
            const string sql = @"
                SELECT RTRIM(cod_justificacion) AS item,
                       RTRIM(descripcion)       AS descripcion
                FROM   Activos_justificaciones
                WHERE  tipo = @tipo";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                sql,
                new { tipo });
        }

        /// <summary>
        /// Método para obtener los proveedores de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Proveedores_Obtener(
            int CodEmpresa)
        {
            const string sql = @"
                SELECT cod_proveedor AS item,
                       descripcion   AS descripcion
                FROM   Activos_proveedores";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Método para consultar listado de activos disponibles para Adición o Retiro
        /// </summary>
        public ErrorDto<List<ActivosData>> Activos_AdicionRetiro_Activos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM   Activos_Principal";

            return DbHelper.ExecuteListQuery<ActivosData>(
                _portalDB,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Método para consultar retiros/adiciones por número de placa e id
        /// </summary>
        public ErrorDto<ActivosRetiroAdicionData> Activos_AdicionRetiro_Consultar(
            int CodEmpresa,
            int Id_AddRet,
            string placa)
        {
            const string sql = @"
                SELECT X.*,
                       RTRIM(J.cod_justificacion) AS Motivo_Id,
                       RTRIM(J.descripcion)       AS Motivo_Desc,
                       A.nombre,
                       P.cod_proveedor,
                       P.descripcion              AS Proveedor
                FROM   Activos_retiro_adicion  X
                       INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                       INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                       LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                WHERE  X.Id_AddRet  = @Id_AddRet
                AND    X.num_placa  = @placa";

            var resp = DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new ActivosRetiroAdicionData(),
                new { Id_AddRet, placa });

            // Ajuste de tipo_vidautil si la consulta fue exitosa
            if (resp.Code == 0 && resp.Result != null && resp.Result.tipo_vidautil != "R")
            {
                resp.Result.tipo_vidautil = "S";
            }

            return resp;
        }

        /// <summary>
        /// Método para validar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        public ErrorDto<string> Activos_AdicionRetiro_Validar(
            int CodEmpresa,
            string placa,
            DateTime fecha)
        {
            var result = DbHelper.CreateOkResponse(string.Empty);

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlFecha = @"
                    SELECT fecha_adquisicion
                    FROM   Activos_Principal
                    WHERE  num_placa = @placa
                    AND    estado   <> 'R'";

                var fechaAdqStr = connection
                    .Query<string>(sqlFecha, new { placa })
                    .FirstOrDefault();

                if (fechaAdqStr == null)
                {
                    result.Code        = -2;
                    result.Description = "El Activo no existe, o ya fue retirado ...";
                }
                else
                {
                    var fechaAdquisicion = DateTime.Parse(
                        fechaAdqStr,
                        System.Globalization.CultureInfo.InvariantCulture);

                    if ((fecha - fechaAdquisicion).Days < 1)
                    {
                        result.Code        = -2;
                        result.Description = "La fecha del Movimiento no es válida, ya que es menor a la del activo ...";
                    }
                }

                const string sqlPeriodo = @"
                    SELECT estado,
                           dbo.fxActivos_PeriodoActual() AS PeriodoActual
                    FROM   Activos_periodos
                    WHERE  anio = @anno
                    AND    mes  = @mes";

                var periodo = connection
                    .Query<ActivosPeriodosData>(
                        sqlPeriodo,
                        new { anno = fecha.Year, mes = fecha.Month })
                    .FirstOrDefault();

                if (periodo != null)
                {
                    if (periodo.estado.Trim() != "P")
                    {
                        result.Code        = -2;
                        result.Description = $"{result.Description} - El Periodo del Movimiento ya fue cerrado ... ";
                    }

                    if (fecha.Year != periodo.periodoactual.Year ||
                        fecha.Month != periodo.periodoactual.Month)
                    {
                        result.Code        = -2;
                        result.Description = $"{result.Description} - La fecha de aplicación del movimiento no corresponde al periodo abierto!";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Método para consultar los meses de un registro de Adición/Retiro
        /// </summary>
        public ErrorDto<int> Activos_AdicionRetiro_Meses_Consulta(
            int CodEmpresa,
            string placa,
            string tipo,
            DateTime fecha)
        {
            const string sql = @"SELECT dbo.fxActivos_VidaUtilPendiente(@placa, @tipo, @fecha)";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                new { placa, tipo, fecha });
        }

        /// <summary>
        /// Método para consultar los datos de Depreciación del Activo al corte
        /// </summary>
        public ErrorDto<ActivosPrincipalData> Activos_AdicionRetiro_DatosActivo_Consultar(
            int CodEmpresa,
            string placa)
        {
            var result = DbHelper.CreateOkResponse(new ActivosPrincipalData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"EXEC spActivos_InfoDepreciacion @placa";

                var data = connection
                    .Query<ActivosPrincipalData>(sql, new { placa })
                    .FirstOrDefault();

                if (data == null)
                {
                    data = new ActivosPrincipalData
                    {
                        depreciacionPeriodo = "????",
                        depreciacion_acum   = 0,
                        valor_historico     = 0,
                        valor_desecho       = 0,
                        valor_libros        = 0
                    };
                }
                else
                {
                    data.depreciacionPeriodo = data.depreciacion_periodo.ToString("dd/MM/yyyy");
                }

                result.Result = data;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        /// <summary>
        /// Método para guardar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        public ErrorDto Activos_AdicionRetiro_Guardar(
            int CodEmpresa,
            string usuario,
            ActivosRetiroAdicionData data)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    EXEC spActivos_AdicionRetiro
                         @Placa,
                         @Tipo,
                         @Justificacion,
                         @Descripcion,
                         @Fecha,
                         @Monto,
                         @Meses,
                         @Usuario,
                         @CompraDoc,
                         @CompraProv,
                         @VentaDoc,
                         @VentaCliente";

                var linea = connection.Query<int>(sql, new
                {
                    Placa         = data.num_placa,
                    Tipo          = data.tipo,
                    Justificacion = data.cod_justificacion,
                    Descripcion   = data.descripcion,
                    Fecha         = data.fecha,
                    Monto         = data.monto,
                    Meses         = data.meses_calculo,
                    Usuario       = usuario,
                    CompraDoc     = data.compra_documento,
                    CompraProv    = data.proveedor,
                    VentaDoc      = data.venta_documento,
                    VentaCliente  = data.venta_cliente
                }).FirstOrDefault();

                result.Code = linea;

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"{data.tipoDescripcion} (Placa: {data.num_placa}) Id: {data.tipoDescripcion}:{data.id_addret}_ {data.justificacion} ",
                    "Registra - WEB");
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para consultar el histórico de retiros y adiciones de un activo
        /// </summary>
        public ErrorDto<List<ActivosHistoricoData>> Activos_AdicionRetiro_Historico_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string sql = @"
                SELECT X.id_AddRet,
                       X.fecha,
                       X.MONTO,
                       X.DESCRIPCION,
                       RTRIM(J.cod_justificacion) + ' - ' + J.descripcion AS Justifica,
                       CASE 
                           WHEN X.Tipo = 'A' THEN 'Adicion/Mejora'
                           WHEN X.Tipo = 'M' THEN 'Mantenimiento'
                           WHEN X.Tipo = 'R' THEN 'Retiro'
                           ELSE ''
                       END AS TipoMov
                FROM   Activos_retiro_adicion X
                       INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                       INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                       LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                WHERE  X.num_placa = @placa
                AND    X.Tipo IN ('A','R','M')";

            return DbHelper.ExecuteListQuery<ActivosHistoricoData>(
                _portalDB,
                CodEmpresa,
                sql,
                new { placa });
        }

        /// <summary>
        /// Método para consultar los cierres de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        public ErrorDto<List<ActivosRetiroAdicionCierreData>> Activos_AdicionRetiro_Cierres_Consultar(
            int CodEmpresa,
            string placa,
            int Id_AddRet)
        {
            const string sql = @"
                SELECT A.*
                FROM   Activos_Auxiliar_Adiciones A
                       INNER JOIN Activos_Periodos P
                           ON A.anio = P.Anio AND A.mes = P.mes
                WHERE  A.num_placa  = @placa
                AND    A.ID_AddRet  = @Id_AddRet
                AND    P.Estado     = 'C'
                ORDER BY A.anio DESC, A.mes DESC";

            return DbHelper.ExecuteListQuery<ActivosRetiroAdicionCierreData>(
                _portalDB,
                CodEmpresa,
                sql,
                new { placa, Id_AddRet });
        }

        /// <summary>
        /// Método para obtener el nombre del activo por número de placa
        /// </summary>
        public ErrorDto<string> Activos_AdicionRetiro_ActivosNombre_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string sql = @"
                SELECT nombre
                FROM   Activos_Principal
                WHERE  num_placa = @placa";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { placa });
        }

        /// <summary>
        /// Método para eliminar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        public ErrorDto Activos_AdicionRetiro_Eliminar(
            int CodEmpresa,
            string placa,
            int Id_AddRet)
        {
            const string sql = @"
                DELETE Activos_retiro_adicion
                WHERE  num_placa = @placa
                AND    Id_AddRet = @Id_AddRet";

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new { placa, Id_AddRet });
        }

        /// <summary>
        /// Consulta el periodo pendiente
        /// </summary>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(
            int CodEmpresa,
            int contabilidad)
        {
            var result = DbHelper.CreateOkResponse(DateTime.Now);

            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}