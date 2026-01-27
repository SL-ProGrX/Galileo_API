using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXEmpresaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXEmpresaDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCntXEmpresaDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene los datos de la empresa contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CntXEmpresaDto> CntXEmpresa_Obtener(int codEmpresa)
        {
            const string query = @"select * from CntX_Empresa_Registro";
            var result = DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, query, new CntXEmpresaDto());
            result.Result ??= new CntXEmpresaDto();
            return result!;
        }

        /// <summary>
        /// Guardar los datos de la empresa contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXEmpresa_Guardar(int codEmpresa, string usuario, CntXEmpresaDto request)
        {
            const string sqlDelete = @"delete CntX_Empresa_Registro;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { }
            );

            if (respDelete != null && respDelete.Code < 0)
                return respDelete;

            const string sqlInsert = @"
                insert into CntX_Empresa_Registro
                (nombre, cedula_juridica, direccion, apto_postal, email, telefono, fax, contacto)
                values
                (@Nombre, @CedulaJuridica, @Direccion, @AptoPostal, @Email, @Telefono, @Fax, @Contacto);";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    Nombre = (request.nombre ?? string.Empty).ToUpperInvariant(),
                    CedulaJuridica = request.cedula_juridica,
                    Direccion = (request.direccion ?? string.Empty).ToUpperInvariant(),
                    AptoPostal = request.apto_postal,
                    Email = request.email,
                    Telefono = request.telefono,
                    Fax = request.fax,
                    Contacto = (request.contacto ?? string.Empty).ToUpperInvariant()
                }
            );

            if (respInsert != null && respInsert.Code < 0)
                return respInsert; 

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Empresa : {request.nombre}",
                Movimiento = "Registra - WEB",
                Modulo = 5
            });

            return new ErrorDto { Code = 0, Description = "Empresa registrada satisfactoriamente." };
        }

    }
}
