using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmCxPTrasladoAsientosDB
    {
        private readonly IConfiguration _config;
        mProGrX_AuxiliarDB DBAuxiliar;
        MSecurityMainDb DBBitacora;
        public frmCxPTrasladoAsientosDB(IConfiguration config)
        {
            _config = config;
            DBAuxiliar = new mProGrX_AuxiliarDB(_config);
            DBBitacora = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene los documentos pendientes de traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Inicio"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<DocsPendientesTraslado> DocPendientes_Obtener(int CodEmpresa, string Inicio, string Corte)
        {

            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var query = string.Empty;
            var response = new ErrorDto<DocsPendientesTraslado>
            {
                Code = 0,
                Result = new DocsPendientesTraslado()
            };


            if (DateTime.TryParse(Inicio, out DateTime dateI))
            {
                dateI = dateI.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
                Inicio = dateI.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            if (DateTime.TryParse(Corte, out DateTime dateC))
            {
                dateC = dateC.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                Corte = dateC.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"SELECT count(*) FROM CxP_Facturas 
                            WHERE Asiento_Generado = 'P' 
                            AND Fecha BETWEEN '{Inicio}' AND '{Corte}'
                            AND dbo.fxCxP_AsientoBalanceado('factura',COD_PROVEEDOR, COD_FACTURA) = 1";

                    response.Result.Facturas_Registradas = connection.Query<int>(query).FirstOrDefault();


                    query = $@"SELECT count(*) FROM CxP_Facturas 
                            WHERE Estado = 'A' AND anula_asiento_fecha IS NULL 
                            AND anula_fecha BETWEEN '{Inicio}' AND '{Corte}'
                            AND dbo.fxCxP_AsientoBalanceado('factura',COD_PROVEEDOR, COD_FACTURA) = 1";

                    response.Result.Facturas_Anuladas = connection.Query<int>(query).FirstOrDefault();


                    query = $@"SELECT count(*) FROM cxP_cargosPer C 
                            INNER JOIN cxp_cargos T ON C.cod_Cargo = T.cod_Cargo
                            INNER JOIN cxp_proveedores P ON C.cod_Proveedor = P.cod_Proveedor
                            WHERE C.Tipo = 'M' AND C.concepto NOT IN('*** PAGO ANTICIPADO ***')
                            AND C.REGISTRO_FECHA BETWEEN '{Inicio}' AND '{Corte}'
                            AND C.Asiento_Fecha IS NULL";

                    response.Result.Cargos_Flotante_Monto = connection.Query<int>(query).FirstOrDefault();


                    query = $@"SELECT count(*) FROM cxp_PagoProvCargos Car 
                            INNER JOIN CXP_CARGOSPER Per ON Car.COD_PROVEEDOR = Per.COD_PROVEEDOR AND Car.ID = Per.ID
                            INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                            INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                            WHERE Per.TIPO = 'P' AND Car.TIPO_PROCESO = 'F' AND Car.Asiento_Fecha IS NULL
                            AND Car.REGISTRO_FECHA BETWEEN '{Inicio}' AND '{Corte}'";

                    response.Result.Cargos_Flotante_Porc = connection.Query<int>(query).FirstOrDefault();


                    query = $@"SELECT count(*) FROM cxp_PagoProvCargos Car  
                            INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                            INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                            WHERE Car.TIPO_PROCESO = 'D' AND Car.Asiento_Fecha IS NULL AND isnull(Car.ID,0) = 0
                            AND Car.REGISTRO_FECHA BETWEEN '{Inicio}' AND '{Corte}' AND Car.Asiento_Fecha IS NULL";

                    response.Result.Cargos_Directos_Factura = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT count(*) FROM CXP_PAGOPROV Pg 
                            INNER JOIN cxp_PagoProvCargos Car ON Pg.COD_PROVEEDOR = Car.COD_PROVEEDOR AND Pg.COD_FACTURA = Car.COD_FACTURA AND Pg.NPAGO = Car.NPAGO
                            INNER JOIN CXP_ANTICIPOS At ON PG.COD_PROVEEDOR = At.COD_PROVEEDOR AND At.ID_CARGO = Car.ID
                            WHERE Pg.TIPO_CANCELACION = 'C'
                            AND Car.ASIENTO_FECHA IS NULL AND isnull(Car.ID,0) > 0
                            AND Car.REGISTRO_FECHA BETWEEN '{Inicio}' AND '{Corte}'";

                    response.Result.Cargos_Flotantes_CobFactCancel_RetCargo = connection.Query<int>(query).FirstOrDefault();


                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los documentos desbalanceados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Inicio"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<List<Desbalanceado>> Desbalanceados_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Desbalanceado>>();

            if (DateTime.TryParse(Inicio, out DateTime dateI))
            {
                dateI = dateI.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
                Inicio = dateI.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            if (DateTime.TryParse(Corte, out DateTime dateC))
            {
                dateC = dateC.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                Corte = dateC.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT 'Factura' AS Tipo,cod_factura AS 'Transacccion', creacion_fecha AS 'Fecha',
                                Creacion_User AS 'Usuario', Total AS 'Monto','Proveedor.: ' + convert(varchar(30), cod_proveedor) AS 'Referencia', Notas
                                FROM CxP_Facturas
                                WHERE Asiento_Generado = 'P' AND Fecha BETWEEN '{Inicio}' AND '{Corte}'
                                AND dbo.fxCxP_AsientoBalanceado('factura',COD_PROVEEDOR, COD_FACTURA) = 0
                                UNION 
                                SELECT 'Factura' AS Tipo,cod_factura AS 'Transacccion', Anula_fecha AS 'Fecha', 
                                Anula_User AS 'Usuario', Total AS 'Monto','Proveedor.: ' + convert(varchar(30), cod_proveedor) AS 'Referencia', Notas 
                                FROM CxP_Facturas 
                                WHERE Estado = 'A' AND anula_asiento_fecha IS NULL AND anula_fecha BETWEEN '{Inicio}' AND '{Corte}'
                                AND dbo.fxCxP_AsientoBalanceado('factura',COD_PROVEEDOR, COD_FACTURA) = 0
                                ORDER BY FECHA";

                    response.Result = connection.Query<Desbalanceado>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Reactivar los documentos procesados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Inicio"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto Reactivar(int CodEmpresa, string Inicio, string Corte)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spSys_Asiento_Revisa_Traslado]";
                    var values = new
                    {
                        Inicio = Inicio,
                        Corte = Corte,
                        Auxiliar = "CxP",
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Valida que el periodo del asiento este abierto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Fecha"></param>
        /// <returns></returns>
        public bool fxValidaPeriodoAsiento(int CodEmpresa, string Fecha)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool result = false;
            List<Periodo> info = new List<Periodo>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT  * FROM CntX_periodos 
                                WHERE anio = Year('{Fecha}')
                                AND mes = Month('{Fecha}')
                                AND estado = 'P' and cod_contabilidad = 1";

                    info = connection.Query<Periodo>(query).ToList();

                    if (info.Count > 0)
                    {
                        result = true;
                    }

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Elimina los casos de cargos flotantes con monto cero
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto CasosCero_Borrar(int CodEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete CXP_PAGOPROVCARGOS where MONTO = 0";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Procesa el traslado de asientos individual
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto AsientoIndividual_Procesar(int CodEmpresa, AsientoInfo data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var strSQL = string.Empty;

            ErrorDto<ParametroValor> UnidadInfo = new ErrorDto<ParametroValor>();
            List<TrasladoData> info = new List<TrasladoData>();


            ErrorDto resp = new ErrorDto();

            try
            {
                if (DateTime.TryParse(data.Inicio, out DateTime dateI))
                {
                    dateI = dateI.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
                    data.Inicio = dateI.ToString("yyyy-MM-ddTHH:mm:ss");
                }

                if (DateTime.TryParse(data.Corte, out DateTime dateC))
                {
                    dateC = dateC.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    data.Corte = dateC.ToString("yyyy-MM-ddTHH:mm:ss");
                }

                using var connection = new SqlConnection(stringConn);
                {
                    UnidadInfo = DBAuxiliar.fxCxPParametro(CodEmpresa, "01");

                    switch (data.vTipoDoc)
                    {
                        case "FT": // Factura Registrada
                            strSQL = $@"SELECT Ft.*,Ft.cod_factura AS Cod_Transaccion, Ft.Fecha AS Registro_Fecha
                                     ,ISNULL(Ft.Creacion_User,'') + char(13) + char(10) + Prov.Descripcion + ' -> ' + Ft.Notas AS AsientoNotas
                                     ,convert(varchar(10),Ft.COD_PROVEEDOR) + '..' + Prov.Descripcion AS Referencia
                                     ,'Factura No.:' + Ft.cod_factura + '.. Prov:' + CONVERT(varchar(10),Ft.COD_PROVEEDOR)  AS AsientoDesc
                                     FROM CxP_Facturas Ft INNER JOIN CxP_Proveedores Prov ON Ft.cod_Proveedor = Prov.cod_Proveedor
                                     WHERE Ft.Asiento_Generado = 'P' AND Ft.Fecha BETWEEN '{data.Inicio}' AND '{data.Corte}'";

                            if (data.chkBalanceados == true)
                            {
                                strSQL += " AND dbo.fxCxP_AsientoBalanceado('factura',Ft.COD_PROVEEDOR, Ft.COD_FACTURA) = 1";
                            }

                            strSQL += " ORDER BY Creacion_Fecha";


                            info = connection.Query<TrasladoData>(strSQL).ToList();


                            break;

                        case "FA": // Factura Anulada
                            strSQL = $@"SELECT Ft.*, Ft.cod_factura AS Cod_Transaccion, Ft.anula_fecha AS Registro_Fecha
                                      ,ISNULL(Ft.Creacion_User,'') + CHAR(13) + CHAR(10) + Prov.Descripcion + ' -> ' + Ft.Notas AS AsientoNotas
                                      ,CONVERT(VARCHAR(10), Ft.COD_PROVEEDOR) + '..' + Prov.Descripcion AS Referencia
                                      ,'Factura Anulada No.:' + Ft.cod_factura + '.. Prov:' + CONVERT(VARCHAR(10), Ft.COD_PROVEEDOR) AS AsientoDesc
                                      FROM CxP_Facturas Ft
                                      INNER JOIN CxP_Proveedores Prov ON Ft.cod_Proveedor = Prov.cod_Proveedor
                                      WHERE Ft.Estado = 'A' AND Ft.anula_asiento_fecha IS NULL AND Ft.anula_fecha BETWEEN '{data.Inicio}' AND '{data.Corte}'";

                            if (data.chkBalanceados == true)
                            {
                                strSQL += " AND dbo.fxCxP_AsientoBalanceado('factura', Ft.COD_PROVEEDOR, Ft.COD_FACTURA) = 1";
                            }

                            strSQL += " ORDER BY anula_fecha";

                            info = connection.Query<TrasladoData>(strSQL).ToList();

                            break;

                        case "CM": // Cargos Flotante base Monto
                            strSQL = $@"SELECT C.*, CONVERT(VARCHAR(10), [ID]) AS Cod_Transaccion, C.concepto AS Descripcion, C.detalle AS AsientoNotas
                                    , T.descripcion AS Cargo, T.cod_cuenta AS CtaCargo, P.cod_cuenta AS CtaProveedor
                                    , T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), C.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), C.COD_PROVEEDOR) AS AsientoDesc
                                    , CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS Referencia
                                    FROM cxP_cargosPer C
                                    INNER JOIN cxp_cargos T ON C.cod_Cargo = T.cod_Cargo
                                    INNER JOIN cxp_proveedores P ON C.cod_Proveedor = P.cod_Proveedor
                                    WHERE C.Tipo = 'M' AND C.concepto NOT IN ('*** PAGO ANTICIPADO ***')
                                    AND C.REGISTRO_FECHA BETWEEN '{data.Inicio}' AND '{data.Corte}' AND C.Asiento_Fecha IS NULL
                                    ORDER BY C.REGISTRO_FECHA";

                            info = connection.Query<TrasladoData>(strSQL).ToList();


                            break;

                        case "CP": // Cargos Flotante Base Porcentual
                            strSQL = $@"SELECT Car.*, CONVERT(VARCHAR(10), Car.[ID]) + '.' + RTRIM(Car.cod_Factura) AS 'Cod_Transaccion'
                                    , Per.concepto AS 'Descripcion'
                                    , CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS 'Referencia'
                                    ,'Cargo de Anticipo/Fact.Cancelada v�a Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Pg.NPago) AS 'AsientoNotas'
                                    , T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS 'AsientoDesc'
                                    , T.descripcion AS 'Cargo', T.cod_cuenta AS 'CtaCargo', P.cod_cuenta AS 'CtaProveedor'
                                    FROM cxp_PagoProvCargos Car
                                    INNER JOIN CXP_CARGOSPER Per ON Car.COD_PROVEEDOR = Per.COD_PROVEEDOR AND Car.ID = Per.ID
                                    INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                                    INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                                    WHERE Per.TIPO = 'P' AND Car.TIPO_PROCESO = 'F' AND Car.Asiento_Fecha IS NULL
                                    AND Car.REGISTRO_FECHA BETWEEN '{data.Inicio}' AND '{data.Corte}'
                                    ORDER BY Car.REGISTRO_FECHA";
                            break;

                        case "CD": // Cargos Directos de la Factura
                            strSQL = $@"SELECT Car.*, CONVERT(VARCHAR(10), Car.[IDX_Consec]) + '.' + RTRIM(Car.cod_Factura) AS 'Cod_Transaccion'
                                    ,'Cargo de Anticipo/Fact.Cancelada v�a Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Car.NPago) AS 'AsientoNotas'
                                    , T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS 'AsientoDesc'
                                    , T.descripcion AS 'Detalle', T.cod_cuenta AS 'CtaCargo', P.cod_cuenta AS 'CtaProveedor'
                                    , CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS 'Referencia'
                                   FROM cxp_PagoProvCargos Car  
                                   INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                                   INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                                   WHERE Car.TIPO_PROCESO = 'D' AND Car.Asiento_Fecha IS NULL AND ISNULL(Car.ID, 0) = 0
                                   AND Car.REGISTRO_FECHA BETWEEN '{data.Inicio}' AND '{data.Corte}'
                                   ORDER BY Car.REGISTRO_FECHA";

                            info = connection.Query<TrasladoData>(strSQL).ToList();


                            break;

                        case "CA": // Cargo de Anticipo de Factura Cancelada v�a Cargo/Retenci�n
                            strSQL = $@"SELECT Car.*, CONVERT(VARCHAR(10), Car.[ID]) + '.' + RTRIM(Car.cod_Factura) AS 'Cod_Transaccion'
                                    ,'Cargo de Anticipo/Fact.Cancelada v�a Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Pg.NPago) AS 'AsientoNotas'
                                    , T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS 'AsientoDesc'
                                    , T.descripcion AS 'Detalle', T.cod_cuenta AS 'CtaCargo', P.cod_cuenta AS 'CtaProveedor'
                                    , CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS 'Referencia'
                                   FROM CXP_PAGOPROV Pg
                                   INNER JOIN cxp_PagoProvCargos Car ON Pg.COD_PROVEEDOR = Car.COD_PROVEEDOR
                                       AND Pg.COD_FACTURA = Car.COD_FACTURA AND Pg.NPAGO = Car.NPAGO AND ISNULL(Car.ID, 0) > 0
                                   INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                                   INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                                   INNER JOIN CXP_ANTICIPOS At ON PG.COD_PROVEEDOR = At.COD_PROVEEDOR AND At.ID_CARGO = Car.ID
                                   WHERE Pg.TIPO_CANCELACION = 'C'
                                   AND Car.ASIENTO_FECHA IS NULL AND ISNULL(Car.ID, 0) > 0
                                   AND Car.REGISTRO_FECHA BETWEEN '{data.Inicio}' AND '{data.Corte}'
                                   ORDER BY Car.REGISTRO_FECHA";

                            info = connection.Query<TrasladoData>(strSQL).ToList();

                            break;
                    }

                    foreach (TrasladoData inf in info)
                    {

                        if (fxValidaPeriodoAsiento(CodEmpresa, inf.Registro_Fecha))
                        {
                            string vNumAsiento;


                            if (data.vMascara != "")
                            {
                                vNumAsiento = data.vTipoDoc + "." + $"{inf.Cod_Proveedor:D2}" + "." + string.Format("{0:" + data.vMascara + "}", inf.Cod_Transaccion);
                            }
                            else
                            {
                                vNumAsiento = data.vTipoDoc + "." + $"{inf.Cod_Proveedor:D2}" + "." + inf.Cod_Transaccion;
                            }


                            strSQL = $@"INSERT INTO CntX_Asientos(cod_contabilidad, Tipo_Asiento, Num_Asiento, Anio, Mes, Fecha_Asiento, descripcion, balanceado, modulo, notas, referencia) 
                                    VALUES (1, '{data.vTipoAsiento}', '{vNumAsiento}', Year('{inf.Registro_Fecha}'), Month('{inf.Registro_Fecha}') , 
                                    '{inf.Registro_Fecha}', '{inf.AsientoDesc.Trim().Substring(0, Math.Min(60, inf.AsientoDesc.Trim().Length))}', 
                                    'S', {30}, '{inf.AsientoNotas}', 
                                    '{(inf.Referencia.Length > 200 ? inf.Referencia.Substring(0, 200) : inf.Referencia)}')";


                            resp.Code = connection.Query<int>(strSQL).FirstOrDefault();

                            switch (data.vTipoDoc)
                            {
                                case "FT": // Factura Registrada
                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                (SELECT Asi.COD_CONTABILIDAD, '{data.vTipoAsiento}', '{vNumAsiento}', Asi.LINEA, Asi.COD_CUENTA, 
                                                CASE WHEN Asi.DebeHaber IN ('D') THEN Asi.MONTO ELSE 0 END, 
                                                CASE WHEN Asi.DebeHaber NOT IN ('D') THEN Asi.MONTO ELSE 0 END, 
                                                'Prov.' + CONVERT(varchar(10), Tra.cod_proveedor) + '.Fact.' + RTRIM(Tra.cod_factura), 
                                                ISNULL(Tra.Cod_Factura, ''), Asi.COD_UNIDAD, Asi.COD_DIVISA, Asi.Tipo_Cambio, Asi.COD_CENTRO_COSTO 
                                                FROM cxp_facturas Tra 
                                                INNER JOIN cxp_facturas_detalle Asi ON Tra.cod_proveedor = Asi.cod_proveedor 
                                                AND Tra.cod_factura = Asi.cod_factura 
                                                WHERE Tra.cod_proveedor = {inf.Cod_Proveedor} AND Tra.cod_factura = '{inf.Cod_Factura}')";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();

                                    strSQL = $@"UPDATE cxp_facturas SET asiento_fecha = Getdate(), asiento_generado = 'G' 
                                                WHERE cod_proveedor = {inf.Cod_Proveedor} AND cod_factura = '{inf.Cod_Factura}'";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();
                                    resp.Description = "Factura registrada correctamente";

                                    break;

                                case "FA": // Factura Anulada
                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                (SELECT Asi.COD_CONTABILIDAD, '{data.vTipoAsiento}', '{vNumAsiento}', Asi.LINEA, Asi.COD_CUENTA, 
                                                CASE WHEN Asi.DebeHaber IN ('D') THEN 0 ELSE Asi.MONTO END, 
                                                CASE WHEN Asi.DebeHaber NOT IN ('D') THEN 0 ELSE Asi.MONTO END, 
                                                'Prov.' + CONVERT(varchar(10), Tra.cod_proveedor) + '.Fact.' + RTRIM(Tra.cod_factura), 
                                                ISNULL(Tra.Cod_Factura, ''), Asi.COD_UNIDAD, Asi.COD_DIVISA, Asi.Tipo_Cambio, Asi.COD_CENTRO_COSTO 
                                                FROM cxp_facturas Tra 
                                                INNER JOIN cxp_facturas_detalle Asi ON Tra.cod_proveedor = Asi.cod_proveedor 
                                                AND Tra.cod_factura = Asi.cod_factura 
                                                WHERE Tra.cod_proveedor = '{inf.Cod_Proveedor}' AND Tra.cod_factura = '{inf.Cod_Factura}')";


                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();

                                    strSQL = $@"UPDATE cxp_facturas SET anula_asiento_fecha = Getdate() 
                                                WHERE cod_proveedor = {inf.Cod_Proveedor} AND cod_factura = '{inf.Cod_Factura}'";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();
                                    resp.Description = "Factura anulada correctamente";

                                    break;

                                case "CM": // Cargos Flotante Base Monto
                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                VALUES (1, '{data.vTipoAsiento}', '{vNumAsiento}', 1, '{inf.CtaProveedor}', 
                                                {inf.Valor}, 0, '{inf.Detalle.Substring(0, Math.Min(100, inf.Detalle.Length))}', 
                                                '{$"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}.{inf.Cod_Cargo.Trim()}"}', 
                                                '{UnidadInfo.Result.Valor}', '{inf.Cod_Divisa}', {inf.Tipo_Cambio}, '')";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();

                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                VALUES (1, '{data.vTipoAsiento}', '{vNumAsiento}', 2, '{inf.CtaCargo}', 
                                                0, {inf.Valor}, '{inf.Detalle.Substring(0, Math.Min(100, inf.Detalle.Length))}', 
                                                '{$"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}.{inf.Cod_Cargo.Trim()}"}', 
                                                '{UnidadInfo.Result.Valor}', '{inf.Cod_Divisa}', {inf.Tipo_Cambio}, '')";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();

                                    strSQL = $@"UPDATE cxP_cargosPer SET asiento_fecha = Getdate(), asiento_usuario = '{data.Usuario}' 
                                                WHERE cod_proveedor = {inf.Cod_Proveedor} AND [ID] = {inf.Id} AND cod_Cargo = '{inf.Cod_Cargo}'";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();
                                    resp.Description = "Ok";

                                    break;

                                case "CP":
                                case "CD":
                                case "CA":
                                    // Code for "CP", "CD", and "CA" cases
                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                VALUES (1, '{data.vTipoAsiento}', '{vNumAsiento}', 1, '{inf.CtaProveedor}', 
                                                {inf.Monto}, 0, '{inf.Detalle.Substring(0, Math.Min(100, inf.Detalle.Length))}', 
                                                '{$"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}"}', 
                                                '{UnidadInfo.Result.Valor}', '{inf.Cod_Divisa}', {inf.Tipo_Cambio}, '')";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();


                                    strSQL = $@"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito, 
                                                detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo) 
                                                VALUES (1, '{data.vTipoAsiento}', '{vNumAsiento}', 2, '{inf.CtaCargo}', 
                                                0, {inf.Monto}, '{inf.Detalle.Substring(0, Math.Min(100, inf.Detalle.Length))}', 
                                                '{$"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}"}', 
                                                '{UnidadInfo.Result.Valor}', '{inf.Cod_Divisa}', {inf.Tipo_Cambio}, '')";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();


                                    strSQL = $@"UPDATE cxp_PagoProvCargos SET asiento_fecha = Getdate(), asiento_usuario = '{data.Usuario}' 
                                                WHERE cod_proveedor =  {inf.Cod_Proveedor} AND IDX_Consec = {inf.IdX_Consec}";

                                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();
                                    resp.Description = "Ok";

                                    break;

                            }

                            if (resp.Code == 0)
                            {
                                Bitacora(new BitacoraInsertarDto
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = data.Usuario,
                                    DetalleMovimiento = "Traslada Asientos: " + DateTime.Parse(data.Inicio).ToString("dd/MM/yyyy")
                                    + " - " + DateTime.Parse(data.Corte).ToString("dd/MM/yyyy"),
                                    Movimiento = "APLICA - WEB",
                                    Modulo = 30
                                });
                            }

                            // Continue with the rest of the code
                        }
                        else
                        {
                            resp.Code = -1;
                            resp.Description = "Existen asientos que no pueden ser trasladados porque el periodo fue cerrado...";
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Registra la bitacora
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }
    }
}