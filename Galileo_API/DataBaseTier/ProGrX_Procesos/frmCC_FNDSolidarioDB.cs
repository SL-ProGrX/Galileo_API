using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_FNDSolidarioDB
    {
        private readonly IConfiguration _config;
        mProGrx_Main mProGrx_Main;

        public frmCC_FNDSolidarioDB(IConfiguration config)
        {
            _config = config;
            mProGrx_Main = new mProGrx_Main(_config);
        }

        public ErrorDTO CC_FNDSolidario_ActualizaCasos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            string Codigo = "FBEN";
            int Monto = 800;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Excluye Cuotas de Ex - Socios
                    var query1 = $@"update reg_creditos set estado = 'C',saldo = 0,cuota = 0 
                        where Estado = 'A' and codigo = '{Codigo}' 
                        and cedula in(select cedula from socios where estadoactual <> 'S')";
                    int result1 = connection.Execute(query1);

                    //ACTUALIZA MONTO CASOS ACTUALES
                    var query2 = $@"update reg_creditos set montoapr = {Monto},cuota = {Monto},saldo = {Monto} 
                        where Estado = 'A' and codigo = '{Codigo}' 
                        and cedula in(select cedula from socios where estadoactual = 'S')";
                    int result2 = connection.Execute(query2);

                    if (result1 == 0 && result2 == 0)
                    {
                        resp.Description = "No se encontró ningun registro por actualizar";
                    }
                    else if (result1 == 0 && result2 != 0)
                    {
                        resp.Description = "No se encontraron cuotas de ex-asociados por excluir";
                    }
                    else if (result1 != 0 && result2 == 0)
                    {
                        resp.Description = "No se encontraron montos de casos actuales por actualizar";
                    }
                    else if (result1 != 0 && result2 != 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Actualizacion exitosa";
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

        public ErrorDTO CC_FNDSolidario_ProcesaCasosNuevos(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            string Codigo = "FBEN";
            int Monto = 800;
            decimal proceso = mProGrx_Main.glngFechaCR(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var fxSIFPrmProcesoSig = @$"select dbo.fxSIFPrmProcesoSig({proceso}) as 'Result'";
                    decimal fxFechaProcesoSiguiente = connection.Query<decimal>(fxSIFPrmProcesoSig).FirstOrDefault();

                    var fxSIFPrmProcesoAnt = @$"select dbo.fxSIFPrmProcesoAnt({proceso}) as 'Result'";
                    decimal fxFechaProcesoAnterior = connection.Query<decimal>(fxSIFPrmProcesoAnt).FirstOrDefault();

                    var query = $@"insert into reg_creditos(codigo,id_comite,cedula,montosol,montoapr,monto_girado
                        ,saldo,amortiza,interesc,saldo_mes,cuota,int,interesv,plazo,userrec,userres
                        ,userfor,usertesoreria,tesoreria,fechasol,fechares,fechaforp,fechaforf
                        ,fecha_calculo_int,garantia,primer_cuota,tdocumento,ndocumento,pagare
                        ,firma_deudor,premio,observacion,estado,prideduc,fecult,estadosol,documento_referido)
                         (select UPPER(@Codigo),6,cedula,@Monto,@Monto,0,@Monto,0,0,
                        @Monto,@Monto,0,0,999,@Usuario,@Usuario,@Usuario,@Usuario,CONVERT(varchar, Getdate(), 23),
                        CONVERT(varchar, Getdate(), 23),CONVERT(varchar, Getdate(), 23),CONVERT(varchar, Getdate(), 23),
                        CONVERT(varchar, Getdate(), 23),CONVERT(varchar, Getdate(), 23),'N'
                        ,'N','OT','',0,1,0,'Proceso Automatico Cuota Mantenimiento CR','A',@FechaProcesoSiguiente
                        ,@FechaProcesoAnterior,'F','AUTOMATICO'
                         from socios where estadoactual = 'S' and cedula not in(select cedula from reg_creditos
                         where estado = 'A' and codigo = @Codigo)
                    )";

                    var parameters = new DynamicParameters();
                    parameters.Add("Codigo", Codigo, DbType.String);
                    parameters.Add("Usuario", Usuario, DbType.String);
                    parameters.Add("Monto", Monto, DbType.Int32);
                    parameters.Add("FechaProcesoSiguiente", fxFechaProcesoSiguiente, DbType.Decimal);
                    parameters.Add("FechaProcesoAnterior", fxFechaProcesoAnterior, DbType.Decimal);

                    resp.Code = connection.Execute(query, parameters);

                    if (resp.Code == 0)
                    {
                        resp.Description = "No fue posible procesar nuevos casos";
                    }
                    else
                    {
                        resp.Description = "Casos nuevos agregados exitosamente";
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

        public ErrorDTO CC_FNDSolidario_CancelaCasos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            string Codigo = "FBEN";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Cancela Casos sin deduccion de Aportes por mas de 2 Meses
                    var query = $@"update reg_creditos set estado = 'C',saldo = 0 where estado = 'A' and codigo = '{Codigo}' 
                        and cedula in(select A.cedula From ahorro_consolidado A inner join socios S on A.cedula = S.cedula
                        where S.estadoactual = 'S'  and datediff( m, A.fecAporte, Getdate()) > 2 )";
                    resp.Code = connection.Execute(query);
                    if (resp.Code == 0)
                    {
                        resp.Description = "No se encontraron casos sin deduccion de aportes por mas de 2 meses por cancelar";
                    }
                    else
                    {
                        resp.Description = "Fondo de Beneficio Socual Actualizado Satisfactoriamente...";
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
    }
}