using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;


namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRSeguimientoDocDB
    {
        private readonly IConfiguration _config;

        public FrmCRSeguimientoDocDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Método para aplicar la verificación del documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoDoc_Aplicar(int CodEmpresa, FrmCRSeguimientoDocData documento)
        {
            if (documento is null)
            {
                return DbHelper.ErrorResponse("Los datos del documento son requeridos.", -2);
            }

            if (documento.documento == 0 || documento.verificacion == 0)
            {
                return DbHelper.ErrorResponse("No se ha especificado el número del documento...", -1);
            }

            if (documento.documento.ToString().Trim() != documento.verificacion.ToString().Trim())
            {
                return DbHelper.ErrorResponse("El número del documento no concuerda con su verificación...", -1);
            }

            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                @"select isnull(count(*),0) as Existe
                  from Tes_Transacciones
                  where ndocumento = @documento
                    and Tipo = 'CK'
                    and id_banco in(
                        select cod_banco
                        from reg_creditos
                        where id_solicitud = @verificacion)",
                0,
                new
                {
                    documento = documento.documento,
                    verificacion = documento.verificacion
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar documento en tesorería.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                return DbHelper.ErrorResponse("El documento especificado ya existe registrado en Tesorería...", -1);
            }

            return DbHelper.OkResponse("Ok");
        }

        private PortalDB CreatePortalDb() => new(_config);

    }
}