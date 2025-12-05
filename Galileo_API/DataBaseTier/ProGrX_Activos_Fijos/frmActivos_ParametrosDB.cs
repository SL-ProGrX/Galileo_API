using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosParametrosDB
    {
        private readonly PortalDB _portalDB;
        public FrmActivosParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Método para consultar lista de parámetros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Parametros_Contabilidad_Obtener(int CodEmpresa)
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
                var query = $@"select rtrim(cod_Contabilidad) as 'item',rtrim(nombre) as 'descripcion' FROM CntX_Contabilidades";
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
        /// Método para establecer el mes inicial del módulo de activos fijos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_EstablecerMes(int CodEmpresa, DateTime periodo)
        {
            var query = "";
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"select * from Activos_parametros";
                var existe = connection.QueryFirstOrDefault<int>(query);

                if (existe <= 0)
                {
                    result.Code = -2;
                    result.Description = $"No se han guardado los parámetros, debe guardarlos primero y luego establecer el inicio del módulo.";
                }
                else
                {
                    query = $@"UPDATE  Activos_parametros
                                    SET set inicio_anio = @anno
                                   ,inicio_mes =  = @mes";
                    connection.Execute(query, new { anno = periodo.Year, mes = periodo.Month });

                    DateTime vFecha = periodo.AddMonths(-1);

                    query = $@"insert Activos_periodos(anio,mes,estado,asientos,traslado) values(
                                 @anno,@mes,'C','G','G'  )";
                    connection.Execute(query, new { anno = vFecha.Year, mes = vFecha.Month });

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
        /// Método para consultar los parámetros generales de activos fijos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosParametrosData> Activos_Parametros_Consultar(int CodEmpresa)
        {
            var query = "";
            var result = new ErrorDto<ActivosParametrosData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosParametrosData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"select cod_empresa,Enlace_Conta,Enlace_SIFC,REGISTRO_PERIODO_CERRADO,nombre_empresa,
                                    forzar_TipoActivo,registroCompras, tipo_anio,inicio_anio 
                                    from Activos_parametros";
                result.Result = connection.Query<ActivosParametrosData>(query).First();
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
        /// Método para guardar los parámetros generales de activos fijos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select coalesce(count(*),0) as Existe from Activos_parametros";
                var existe = connection.QueryFirstOrDefault<int>(query);

                if (existe > 0)
                {
                    result = Activos_Parametros_Actualizar(CodEmpresa, usuario, datos);
                }
                else
                {
                    result = Activos_Parametros_Insertar(CodEmpresa, usuario, datos);
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
        /// Método para actualizar los parámetros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_Parametros_Actualizar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"UPDATE  Activos_parametros
                                    SET cod_empresa = @cod_empresa,
                                        nombre_empresa = @nombre_empresa,
                                        enlace_conta = @enlace_conta,
                                        enlace_sifc = @enlace_sifc,
                                        tipo_anio = @tipo_anio,
                                        forzar_TipoActivo = @forzar_TipoActivo,
                                        registroCompras = @registroCompras,
                                        REGISTRO_PERIODO_CERRADO = @REGISTRO_PERIODO_CERRADO";
                connection.Execute(query, new
                {
                    datos.cod_empresa,
                    datos.nombre_empresa,
                    datos.enlace_conta,
                    datos.enlace_sifc,
                    datos.tipo_anio,
                    datos.forzar_tipoactivo,
                    datos.registrocompras,
                    datos.registro_periodo_cerrado
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método para insertar los parámetros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_Parametros_Insertar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"insert into Activos_parametros(cod_empresa,nombre_empresa,enlace_conta,enlace_sifc,Tipo_Anio,forzar_TipoActivo,RegistroCompras, REGISTRO_PERIODO_CERRADO)
                                    VALUES (@cod_empresa, @nombre_empresa, @enlace_conta, @enlace_conta, @enlace_sifc, @tipo_anio, @forzar_tipoactivo, @registrocompras, @registro_periodo_cerrado)";
                connection.Execute(query, new
                {
                    datos.cod_empresa,
                    datos.nombre_empresa,
                    datos.enlace_conta,
                    datos.enlace_sifc,
                    datos.tipo_anio,
                    datos.forzar_tipoactivo,
                    datos.registrocompras,
                    datos.registro_periodo_cerrado
                });
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