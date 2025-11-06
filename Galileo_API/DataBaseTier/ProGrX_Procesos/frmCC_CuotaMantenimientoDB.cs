using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_CuotaMantenimientoDB
    {
        private readonly IConfiguration _config;
        mProGrx_Main mProGrx_Main;

        public frmCC_CuotaMantenimientoDB(IConfiguration config)
        {
            _config = config;
            mProGrx_Main = new mProGrx_Main(_config);
        }

        public List<CcCaInstitucionesData> CC_InstitucionesObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CcCaInstitucionesData> resp = new List<CcCaInstitucionesData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_institucion as IdX,rtrim(descripcion) as ItmX 
                        from instituciones where activa = 1 and cod_institucion in(1,2) order by descripcion";
                    resp = connection.Query<CcCaInstitucionesData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CuotaMantenimiento_ActualizaCasos(int CodEmpresa, int Cod_Institucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            string Codigo = "CMCR";
            int Monto = 500;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Excluye Cuotas de Ex - Asociados
                    var query1 = $@"update R set Estado = 'C' 
                        from reg_creditos R inner join Socios S on R.cedula = S.cedula
                        where S.cod_institucion = {Cod_Institucion}
                        and R.codigo = '{Codigo}' and R.estado = 'A'
                        and S.estadoActual not in('S')";
                    int result1 = connection.Execute(query1);

                    //Actualiza Monto Casos Actuales
                    var query2 = $@"update R set montoapr = {Monto} ,cuota = {Monto},saldo = {Monto}
                        from reg_creditos R inner join Socios S on R.cedula = S.cedula
                        where S.cod_institucion = {Cod_Institucion}
                        and R.codigo = '{Codigo}' and R.estado = 'A'
                        and S.estadoActual = 'S'";
                    int result2 = connection.Execute(query2);

                    if (result1 == 0 && result2 == 0)
                    {
                        resp.Description = "No se encontr� ningun registro por actualizar";
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

        public ErrorDto CC_CuotaMantenimiento_ProcesaCasosNuevos(int CodEmpresa, int Cod_Institucion, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            string Codigo = "CMCR";
            int Monto = 500;
            decimal proceso = mProGrx_Main.glngFechaCR(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var fxSIFPrmProcesoSig = @$"select dbo.fxSIFPrmProcesoSig({proceso}) as 'Result'";
                    decimal fxFechaProcesoSiguiente = connection.Query<decimal>(fxSIFPrmProcesoSig).FirstOrDefault();

                    var fxSIFPrmProcesoAnt = @$"select dbo.fxSIFPrmProcesoAnt({proceso}) as 'Result'";
                    decimal fxFechaProcesoAnterior = connection.Query<decimal>(fxSIFPrmProcesoAnt).FirstOrDefault();

                    //Procesando Casos Nuevos
                    var query = $@"
                    insert into reg_creditos (
                            codigo, id_comite, cedula, montosol, montoapr,
                            monto_girado, saldo, amortiza, interesc, saldo_mes,
                            cuota, reg_creditos.INT, interesv, plazo, userrec,
                            userres, userfor, usertesoreria, tesoreria, fechasol,
                            fechares, fechaforp, fechaforf, fecha_calculo_int, garantia,
                            primer_cuota, TDOCUMENTO, ndocumento, pagare, firma_deudor,
                            premio, observacion, estado, prideduc, fecult,
                            estadosol, documento_referido
                     )
                    ( select 
                            UPPER(@Codigo), 6, cedula, @Monto, @Monto,
                            0, @Monto, 0, 0, @Monto,
                            @Monto, 0, 0, 999, @Usuario,
                            @Usuario, @Usuario, @Usuario, CONVERT(varchar, Getdate(), 23), CONVERT(varchar, Getdate(), 23),
                            CONVERT(varchar, Getdate(), 23), CONVERT(varchar, Getdate(), 23), CONVERT(varchar, Getdate(), 23), CONVERT(varchar, Getdate(), 23), 'R',
                            'N', 'OT', ' ', 0, 1,
                            0, 'Proceso Automatico Cuota Mantenimiento CR', 'A', @FechaProcesoSiguiente, @FechaProcesoAnterior,
                            'F', 'AUTOMATICO' 
                        from socios where estadoactual = 'S' and cod_institucion = @CodInstitucion
                        and cedula not in(select cedula from reg_creditos where estado = 'A' and codigo = @Codigo)
                    )";
                    var parameters = new DynamicParameters();
                    parameters.Add("Codigo", Codigo, DbType.String);
                    parameters.Add("Usuario", Usuario, DbType.String);
                    parameters.Add("Monto", Monto, DbType.Int32);
                    parameters.Add("FechaProcesoSiguiente", fxFechaProcesoSiguiente, DbType.Decimal);
                    parameters.Add("FechaProcesoAnterior", fxFechaProcesoAnterior, DbType.Decimal);
                    parameters.Add("CodInstitucion", Cod_Institucion, DbType.Int32);

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
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CuotaMantenimiento_CancelaCasos(int CodEmpresa, int Cod_Institucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            string Codigo = "CMCR";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Cancela Casos en Congelamiento Activado
                    var query1 = $@"update R set Estado = 'C'
                        from reg_creditos R inner join Socios S on R.cedula = S.cedula and S.cod_institucion = {Cod_Institucion} 
                        where R.estado = 'A' and R.codigo = '{Codigo}' 
                        and R.cedula in(select cedula From afi_congelar where estado = 'A'  and fecha_finaliza >= Getdate()
                        and per_cobro_cuotaCr = 0)";
                    int result1 = connection.Execute(query1);

                    //Cancela Casos sin deduccion de Aportes por mas de 2 Meses
                    var query2 = $@"update reg_creditos set estado = 'C' where estado = 'A' and codigo = '{Codigo}' 
                        and cedula in(select A.cedula From ahorro_consolidado A inner join socios S on A.cedula = S.cedula
                        where S.estadoactual = 'S'  and datediff( m, A.fecAporte, Getdate()) > 2
                        and S.cod_institucion = {Cod_Institucion} )";
                    int result2 = connection.Execute(query2);


                    if (result1 == 0 && result2 == 0)
                    {
                        resp.Description = "No se encontr� ningun caso por cancelar";
                    }
                    else if (result1 == 0 && result2 != 0)
                    {
                        resp.Description = "No se encontraron casos en congelamiento activo por cancelar";
                    }
                    else if (result1 != 0 && result2 == 0)
                    {
                        resp.Description = "No se encontraron casos sin deduccion de aportes por mas de 2 meses por cancelar";
                    }
                    else if (result1 != 0 && result2 != 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Cuota de Mantenimiento Actualizada Satisfactoriamente...";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}