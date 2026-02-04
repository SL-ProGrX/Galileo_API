using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.DataBaseTier
{
    public class FrmSifEstadoCuentaTextoCtaCorDB
    {
        private readonly IConfiguration _config;

        public FrmSifEstadoCuentaTextoCtaCorDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene las notas para los estados de cuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<SifEmpresaDto> NotasEstados_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto<SifEmpresaDto> result = new ErrorDto<SifEmpresaDto>();
            result.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT ID_EMPRESA, EC_Nota01, EC_Nota02 FROM sif_empresa";

                result.Result = connection.Query<SifEmpresaDto>(query).FirstOrDefault();
                if (result.Result == null)
                {
                    result.Code = -2;
                    result.Description = "No se encontraron datos";
                }
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
        /// Inserta o actualiza las notas para los estados de cuenta
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="notas"></param>
        /// <returns></returns>
        public ErrorDto NotasEstados_Insertar(int CodCliente, SifEmpresaDto notas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"UPDATE sif_empresa SET 
                                ec_nota01 = '{notas.ec_nota01}'
                                ,ec_nota02 =  '{notas.ec_nota02}'
                                WHERE id_empresa = {notas.id_empresa}";
                connection.Execute(query);
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de beneficio ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }
    }
}