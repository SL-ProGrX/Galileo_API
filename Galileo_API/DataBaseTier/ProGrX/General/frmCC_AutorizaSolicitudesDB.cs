using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCcAutorizaSolicitudesDb
    {
        private readonly IConfiguration _config;

        public FrmCcAutorizaSolicitudesDb(IConfiguration config)
        {
            _config = config;
        }

        public List<CCGenericList> CC_Cuentas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"select ID_BANCO as IdX , rtrim(DESCRIPCION) as itmx
                              from TES_BANCOS 
                              where ESTADO = 'A' and supervision = 1";
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<AutorizaSolicitudesCreditoData> CC_ModuloCredito_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<AutorizaSolicitudesCreditoData> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Convertimos las fechas a DateTime y usamos parámetros
                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1); // 23:59:59.999...

                var query = @"
                    select R.id_solicitud,
                           R.codigo,
                           S.cedula,
                           S.nombre,
                           R.monto_girado
                    FROM reg_creditos R
                    inner join Socios S on R.cedula = S.cedula
                    inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
                    WHERE R.estadosol = 'F'
                      and R.fechaforp between @FechaInicio and @FechaCorte
                      and R.tesoreria is null
                      and R.estado in ('A','C')
                      and id_solicitud not in (select id_solicitud from CRD_REMESAS_TES_DETALLE)
                      and dbo.fxTesSupervisa(S.cedula, S.nombre, R.monto_girado, 0, 'C') = 1
                      and R.TES_SUPERVISION_FECHA is null
                      and (@CodBanco is null or R.cod_banco = @CodBanco);";

                var parameters = new
                {
                    FechaInicio = fechaInicioDt,
                    FechaCorte = fechaCorteDt,
                    CodBanco
                };

                resp = connection.Query<AutorizaSolicitudesCreditoData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<AutorizaSolicitudesFondosData> CC_ModuloFondos_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<AutorizaSolicitudesFondosData> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                var query = @"
                    Select L.Consec,
                           C.Cedula,
                           S.nombre,
                           L.Cod_Plan,
                           L.Cod_Contrato,
                           case
                               when L.Total_Girar is null
                                   then L.Aportes_Liq + L.Rendi_Liq - isnull(L.multa_retiro, 0)
                               else L.Total_Girar
                           end as Total_Girar
                    From Fnd_Liquidacion L
                    inner join Fnd_Contratos C on L.Cod_Operadora = C.Cod_Operadora 
                                              and L.Cod_Plan      = C.Cod_Plan
                                              and L.Cod_Contrato  = C.Cod_Contrato
                    inner join Socios S on C.cedula = S.cedula
                    Where L.Fecha between @FechaInicio and @FechaCorte 
                      And L.Traspaso_tesoreria is Null
                      and L.TES_SUPERVISION_FECHA is null
                      and dbo.fxTesSupervisa(
                            C.cedula,
                            S.nombre,
                            isnull(L.Total_Girar, L.Aportes_Liq + L.Rendi_Liq - isnull(L.multa_retiro, 0)),
                            0,
                            'C'
                          ) = 1
                      and (@CodBanco is null or L.cod_banco = @CodBanco);";

                var parameters = new
                {
                    FechaInicio = fechaInicioDt,
                    FechaCorte = fechaCorteDt,
                    CodBanco
                };

                resp = connection.Query<AutorizaSolicitudesFondosData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<AutorizaSolicitudesLiquidacionData> CC_ModuloLiquidacion_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<AutorizaSolicitudesLiquidacionData> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                var query = @"
                    Select L.consec,
                           S.cedula,
                           S.nombre,
                           L.TNeto,
                           case
                               when L.EstadoActLiq = 'A' then 'Ren.Asociación'
                               when L.EstadoActLiq = 'P' then 'Ren.Patronal'
                           end as Tipo
                    from Liquidacion L
                    inner join Socios S on L.cedula = S.cedula
                    where L.FecLiq between @FechaInicio and @FechaCorte
                      and L.Ubicacion = 'T' 
                      and L.Estado = 'P'
                      and L.TES_SUPERVISION_FECHA is null
                      and dbo.fxTesSupervisa(S.cedula, S.nombre, L.TNeto, 0, 'L') = 1
                      and (@CodBanco is null or L.cod_banco = @CodBanco);";

                var parameters = new
                {
                    FechaInicio = fechaInicioDt,
                    FechaCorte = fechaCorteDt,
                    CodBanco
                };

                resp = connection.Query<AutorizaSolicitudesLiquidacionData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<AutorizaSolicitudesBeneficiosData> CC_ModuloBeneficios_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<AutorizaSolicitudesBeneficiosData> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                var query = @"
                    Select B.Cedula,
                           B.consec,
                           B.cod_beneficio,
                           S.Nombre,
                           B.monto
                    from afi_bene_pago B
                    inner join socios S on B.cedula = S.cedula
                    inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio
                                               and B.consec        = O.consec
                    inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado
                    inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco
                    where O.cod_remesa is null
                      and B.TES_SUPERVISION_FECHA is null
                      and O.registra_fecha between @FechaInicio and @FechaCorte
                      and B.ESTADO = 'S'
                      and B.tesoreria is null
                      and dbo.fxTesSupervisa(B.cedula, S.nombre, B.monto, 0, 'C') = 1
                      and (@CodBanco is null or B.cod_banco = @CodBanco);";

                var parameters = new
                {
                    FechaInicio = fechaInicioDt,
                    FechaCorte = fechaCorteDt,
                    CodBanco
                };

                resp = connection.Query<AutorizaSolicitudesBeneficiosData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<AutorizaSolicitudesHipotecarioData> CC_ModuloHipotecario_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<AutorizaSolicitudesHipotecarioData> resp = [];
            try
            {
                using var connection = new SqlConnection(stringConn);

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                var query = @"
                    select D.CodigoDesembolso,
                           D.NumeroOperacion,
                           D.Beneficiario,
                           D.Monto,
                           D.RegistroFecha,
                           D.RegistroUsuario,
                           S.cedula,
                           S.nombre,
                           R.codigo,
                           D.TES_SUPERVISION_FECHA  
                    From ViviendaDesembolsos D
                    inner join Reg_Creditos R on D.numeroOperacion = R.id_solicitud
                    inner join Socios S on R.cedula = S.cedula
                    where D.TesoreriaRemesa is null
                      and D.TES_SUPERVISION_FECHA is null
                      and D.RegistroFecha between @FechaInicio and @FechaCorte
                      and dbo.fxTesSupervisa(D.Identificacion, D.Beneficiario, D.Monto, 0, 'V') = 1
                      and (@CodBanco is null or D.cod_banco = @CodBanco);";

                var parameters = new
                {
                    FechaInicio = fechaInicioDt,
                    FechaCorte = fechaCorteDt,
                    CodBanco
                };

                resp = connection.Query<AutorizaSolicitudesHipotecarioData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ModuloCredito_Autorizar(int CodEmpresa, string Usuario, int Id_Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    update REG_CREDITOS
                    set TES_SUPERVISION_USUARIO = @Usuario,
                        TES_SUPERVISION_FECHA   = Getdate()
                    where id_solicitud = @Id_Solicitud;";

                resp.Code = connection.Execute(query, new { Usuario, Id_Solicitud });
                resp.Description = "Autorización de operación: " + Id_Solicitud + " fue procesada exitosamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ModuloFondos_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    update Fnd_Liquidacion
                    set TES_SUPERVISION_USUARIO = @Usuario,
                        TES_SUPERVISION_FECHA   = Getdate()
                    where consec = @Consec;";

                resp.Code = connection.Execute(query, new { Usuario, Consec });
                resp.Description = "Autorización de Id: " + Consec + " fue procesada exitosamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ModuloLiquidacion_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    update Liquidacion
                    set TES_SUPERVISION_USUARIO = @Usuario,
                        TES_SUPERVISION_FECHA   = Getdate()
                    where consec = @Consec;";

                resp.Code = connection.Execute(query, new { Usuario, Consec });
                resp.Description = "Autorización de Id " + Consec + " procesada exitosamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ModuloBeneficios_Autorizar(int CodEmpresa, string Usuario, int Consec, string Cod_Beneficio)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    update afi_bene_pago
                    set TES_SUPERVISION_USUARIO = @Usuario,
                        TES_SUPERVISION_FECHA   = Getdate()
                    where consec        = @Consec
                      and cod_beneficio = @Cod_Beneficio;";

                resp.Code = connection.Execute(query, new { Usuario, Consec, Cod_Beneficio });
                resp.Description = "Autorización de Id " + Consec + " procesada exitosamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ModuloHipotecario_Autorizar(int CodEmpresa, string Usuario, int CodigoDesembolso)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    update ViviendaDesembolsos
                    set TES_SUPERVISION_USUARIO = @Usuario,
                        TES_SUPERVISION_FECHA   = Getdate()
                    where CodigoDesembolso = @CodigoDesembolso;";

                resp.Code = connection.Execute(query, new { Usuario, CodigoDesembolso });
                resp.Description = "Autorización de Id " + CodigoDesembolso + " procesada exitosamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}