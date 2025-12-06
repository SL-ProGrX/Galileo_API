using Galileo.Models.ERROR;
using Galileo.Models;
using Dapper;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosReportesDB
    {
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosReportesDB(IConfiguration config)
        {
            _mActivos = new MActivosFijos(config);
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Metodo para consultar listado de departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Departamentos_Obtener(int CodEmpresa)
        {

            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select rtrim(cod_departamento) as 'item',rtrim(descripcion) as 'descripcion' from Activos_departamentos order by cod_departamento";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodos para consultar listados de secciones por departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Secciones_Obtener(int CodEmpresa, string departamento)
        {

            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select rtrim(cod_Seccion) as 'item',rtrim(descripcion) as 'descripcion' FROM  Activos_Secciones where cod_departamento = @departamento order by cod_Seccion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { departamento }).ToList();


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para consultar listado de tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_TipoActivo_Obtener(int CodEmpresa)
        {

            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select rtrim(tipo_activo) as 'item',rtrim(descripcion) as 'descripcion' from Activos_tipo_activo order by tipo_activo";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para consultar listado de localizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Localizacion_Obtener(int CodEmpresa)
        {

            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select rtrim(COD_LOCALIZA) as 'item',rtrim(descripcion) as 'descripcion' from ACTIVOS_LOCALIZACIONES Where Activa = 1 order by descripcion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo pata consultar el estado de  un periodo 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Reportes_PeriodoEstado(int CodEmpresa, DateTime fecha)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "",
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select Estado from Activos_Periodos where Anio = @anno and Mes =@mes";
                var estado = connection.Query<string>(query, new { anno = fecha.Year, mes = fecha.Month }).FirstOrDefault();
                if (estado == null)
                {
                    result.Result = "Periodo No Registrado!";
                }
                else
                {
                    if (estado == "C")
                    {
                        result.Result = "CERRADO";
                    }
                    else
                    {
                        result.Result = "PENDIENTE";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Metodo para consultar el periodo actual
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {


            var result = new ErrorDto<DateTime>
            {
                Code = 0,
                Description = "Ok",
                Result = DateTime.Now,
            };
            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;

            }
            return result;
        }

        /// <summary>
        /// Metodo para consultar listado de responsables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosReportesResponsableData>> Activos_Reportes_Responsables_Consultart(int CodEmpresa)
        {



            var result = new ErrorDto<List<ActivosReportesResponsableData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosReportesResponsableData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select IDENTIFICACION, NOMBRE , Departamento, Seccion  From vActivos_Personas";
                result.Result = connection.Query<ActivosReportesResponsableData>(query).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;

            }
            return result;
        }


    }
}
