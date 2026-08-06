using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaDB
    {
        private readonly PortalDB _portalDB;

        public FrmCxCConsultaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Consulta los datos generales de una persona por cédula o número de operación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula o número de operación ingresado por el usuario.</param>
        /// <returns>Persona encontrada o null cuando no existe.</returns>
        public ErrorDto<CxCPersonaDto?> ConsultarPersona(int codEmpresa, string cedula)
            => DbHelper.ExecuteSingleQuery<CxCPersonaDto>(
                _portalDB,
                codEmpresa,
                @"SELECT
                        P.cedula,
                        P.nombre,
                        C.descripcion AS categoria_desc,
                        dbo.fxCxC_PersonasMsjNum(P.cedula) AS ind_mensajes,
                        ISNULL(Fa.Facturas,0) AS facturas,
                        ISNULL(Fa.Total,0) AS facturas_total
                      FROM CxC_Personas P
                      INNER JOIN CxC_Categoria_Clientes C 
                        ON P.cod_categoria = C.cod_categoria
                      LEFT JOIN vCxC_C_Persona_Facturas_Anio Fa 
                        ON P.cedula = Fa.cedula
                      WHERE P.cedula = @cedula
                         OR P.cedula = (
                            SELECT TOP (1) Cta.cedula
                            FROM CxC_Cuentas Cta
                            WHERE CONVERT(VARCHAR(30), Cta.operacion) = LTRIM(RTRIM(@cedula))
                         )",
                defaultValue: null,
                parameters: new { cedula });

        /// <summary>
        /// Consulta las cuentas de una persona filtradas por estado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <param name="estado">Estado de la operación: activa, cancelada o en trámite.</param>
        /// <returns>Lista de cuentas asociadas a la persona.</returns>
        public ErrorDto<List<CxCCuentaDto>> ConsultarCuentas(int codEmpresa, string cedula, string estado)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn => cn.Query<CxCCuentaDto>(
                    "spCxC_PersonasCuentas",
                    new { cedula, estado },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList());


        /// <summary>
        /// Consulta las solicitudes asociadas a una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <returns>Lista de solicitudes.</returns>
        public ErrorDto<List<CxCSolicitudDto>> ConsultarSolicitudes(int codEmpresa, string cedula)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn => cn.Query<CxCSolicitudDto>(
                    "spSIFEstadoSolicitud",
                    new { cedula },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

        /// <summary>
        /// Consulta los preanálisis asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <returns>Lista de preanálisis.</returns>
        public ErrorDto<List<CxCPreAnalisisDto>> ConsultarPreAnalisis(int codEmpresa, string cedula)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn => cn.Query<CxCPreAnalisisDto>(
                    "spSIFEstadoPreAnalisis",
                    new { cedula },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

        /// <summary>
        /// Consulta las operaciones incobrables asociadas a una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <returns>Lista de operaciones incobrables.</returns>
        public ErrorDto<List<CxCIncobrableDto>> ConsultarIncobrables(int codEmpresa, string cedula)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn => cn.Query<CxCIncobrableDto>(
                    "spSIFEstadoIncobrable",
                    new { cedula },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

        /// <summary>
        /// Consulta los desembolsos asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <returns>Lista de desembolsos con sus referencias de Tesorería.</returns>
        public ErrorDto<List<CxCDesembolsoDto>> ConsultarDesembolsos(int codEmpresa, string cedula)
            => DbHelper.ExecuteListQuery<CxCDesembolsoDto>(
                _portalDB,
                codEmpresa,
                @"SELECT Ct.OPERACION, Cg.MONTO, Td.ESTADO, Td.FECHA_EMISION,
                             Td.TIPO, Cg.Id_Giro, Cg.TESORERIA_SOLICITUD,
                             Cg.TESORERIA_REMESA, Tb.DESCRIPCION AS banco_desc,
                             Td.BENEFICIARIO, Td.NDOCUMENTO
                      FROM CXC_CUENTAS Ct
                      INNER JOIN CXC_CUENTAS_GIROS Cg ON Ct.OPERACION = Cg.OPERACION
                      INNER JOIN TES_TRANSACCIONES Td ON Cg.TESORERIA_SOLICITUD = Td.NSOLICITUD
                      INNER JOIN TES_BANCOS Tb ON Td.ID_BANCO = Tb.ID_BANCO
                      WHERE Ct.CEDULA = @cedula",
                new { cedula });

        /// <summary>
        /// Consulta los mensajes vigentes de una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona consultada.</param>
        /// <returns>Lista de mensajes que no han vencido.</returns>
        public ErrorDto<List<CxCMensajeDto>> ConsultarMensajes(int codEmpresa, string cedula)
            => DbHelper.ExecuteListQuery<CxCMensajeDto>(
                _portalDB,
                codEmpresa,
                @"SELECT
                fecha,
                cedula,
                usuario,
                vencimiento,
                mensaje
              FROM CxC_Personas_Mensajes
              WHERE cedula = @cedula
              AND DATEDIFF(DAY, dbo.MyGetdate(), vencimiento) >= 0",
                new { cedula });

        /// <summary>
        /// Registra un mensaje para la persona consultada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="dto">Datos del mensaje que se registrará.</param>
        /// <returns>True cuando el procedimiento registra el mensaje.</returns>
        public ErrorDto<bool> GuardarMensaje(int codEmpresa, CxCMensajeAddDto dto)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn =>
                {
                    cn.Execute(
                        "CxC_Personas_Mensajes_Insert",
                        dto,
                        commandType: System.Data.CommandType.StoredProcedure);
                    return true;
                });

        /// <summary>
        /// Elimina un mensaje previamente registrado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="dto">Llave del mensaje que se eliminará.</param>
        /// <returns>True cuando el procedimiento elimina el mensaje.</returns>
        public ErrorDto<bool> EliminarMensaje(int codEmpresa, CxCMensajeDeleteDto dto)
            => DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn =>
                {
                    cn.Execute(
                        "CxC_Personas_Mensajes_Delete",
                        dto,
                        commandType: System.Data.CommandType.StoredProcedure);
                    return true;
                });
    
        /// <summary>
        /// Consulta las facturas aplicando los filtros seleccionados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="filtro">Cédula, operación, factura, estado y rango de fechas.</param>
        /// <returns>Lista de facturas que cumplen los filtros.</returns>
        public ErrorDto<List<CxCFacturaDto>> ConsultarFacturas(int codEmpresa, CxCFacturaFiltroDto filtro)
            => DbHelper.WithConn(_portalDB, codEmpresa, cn =>
            {
                var sql = @"
            SELECT 
                Operacion        AS operacion,
                cod_Factura      AS cod_factura,
                Fecha_Pago       AS fecha_pago,
                Monto            AS monto,
                Adelanto_Monto   AS adelanto_monto,
                Liberado         AS liberado,
                Pagador_Nombre   AS pagador_nombre,
                Factura_Estado_Desc AS factura_estado_desc
            FROM vCxC_Facturas_Control
            WHERE cedula = @cedula
              AND cod_Factura LIKE '%' + @cod_factura + '%'
        ";

                if (filtro.operacion.HasValue)
                {
                    sql += " AND operacion = @operacion";
                }

                switch (filtro.tipo_fecha)
                {
                    case "Registro":
                        sql += " AND Registro_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Emisión":
                        sql += " AND fecha_Emision BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Pago":
                        sql += " AND fecha_Pago BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Libera":
                        sql += " AND Liberado_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Cancela":
                        sql += " AND Cancela_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Activación":
                        sql += " AND Activa_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Desembolso 1":
                        sql += " AND Pago_Principal_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Desembolso 2":
                        sql += " AND Pago_Secundario_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;
                }

                if (!string.IsNullOrEmpty(filtro.estado) && filtro.estado != "TODOS")
                {
                    sql += " AND factura_estado = @estado";
                }

                return cn.Query<CxCFacturaDto>(
                    sql,
                    new
                    {
                        filtro.cedula,
                        cod_factura = filtro.cod_factura ?? string.Empty,
                        filtro.operacion,
                        fecha_inicio = filtro.fecha_inicio?.Date,
                        fecha_corte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1),
                        filtro.estado
                    }).ToList();
            });

        /// <summary>
        /// Consulta las facturas asociadas a un giro de desembolso.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="operacion">Número de operación de CxC.</param>
        /// <param name="idGiro">Identificador del giro seleccionado.</param>
        /// <returns>Lista de facturas asociadas al giro.</returns>
        public ErrorDto<List<CxCDesembolsoFacturaDto>> ConsultarFacturasPorGiro(int codEmpresa, int operacion, int idGiro)
            => DbHelper.ExecuteListQuery<CxCDesembolsoFacturaDto>(
                _portalDB,
                codEmpresa,
                @"SELECT
                OPERACION       AS operacion,
                COD_FACTURA     AS cod_factura,
                MONTO           AS monto,
                ADELANTO_MONTO  AS adelanto_monto,
                LIBERADO        AS liberado,
                COD_DIVISA      AS cod_divisa,
                TIPO_CAMBIO     AS tipo_cambio,
                OPERACION_ORIGEN AS operacion_origen
              FROM CXC_CUENTAS_FACTURAS
              WHERE OPERACION = @operacion
              AND @idGiro IN (ID_GIRO, ID_GIRO_PENDIENTE)",
                new { operacion, idGiro });

        /// <summary>
        /// Consulta los estados de factura configurados en la empresa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Estados disponibles para el filtro de facturas.</returns>
        public ErrorDto<List<CxCFacturaEstadoDto>> ConsultarEstadosFactura(int codEmpresa)
            => DbHelper.ExecuteListQuery<CxCFacturaEstadoDto>(
                _portalDB,
                codEmpresa,
                @"SELECT FACTURA_ESTADO AS value,
                         DESCRIPCION AS label
                  FROM CXC_FACTURAS_ESTADOS
                  ORDER BY DESCRIPCION");
    }
}






