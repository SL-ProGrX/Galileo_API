using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesReposicionDB
    {
        private readonly PortalDB _portalDB;
        private readonly int module = 9;
        private readonly MSecurityMainDb mSecurity;
        private readonly MTesoreria mTesoreria;


        public FrmTesReposicionDB(IConfiguration config)
        {
            mSecurity = new MSecurityMainDb(config);
            mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los datos de una solicitud de reposición de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesReposicionData> TES_Reposicion_Obtenet(int CodEmpresa, int solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string query = @"
SELECT
    C.Nsolicitud,
    C.Codigo,
    C.beneficiario,
    C.tipo,
    C.estado,
    C.ndocumento,
    C.id_banco,
    B.descripcion as BancoX,
    T.descripcion as TipoDocX,
    C.Monto,
    C.Fecha_Emision,
    C.Tipo_Beneficiario,
    C.Cta_Ahorros,
    C.Detalle1 + ' ' + C.Detalle2 + ' ' + ISNULL(C.Detalle3,'') + ' ' + ISNULL(C.Detalle4,'') + ' ' + ISNULL(C.Detalle5,'') as Detalle,
    CASE
        WHEN C.Tipo_Beneficiario = 1 THEN 'Personas'
        WHEN C.Tipo_Beneficiario = 2 THEN 'Bancos'
        WHEN C.Tipo_Beneficiario = 3 THEN 'Proveedores'
        WHEN C.Tipo_Beneficiario = 4 THEN 'Acreedores'
    END as TipoBeneficiario,
    ISNULL(C.REPOSICION_IND,0) as ReposicionPaso
FROM Tes_Transacciones C
INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
INNER JOIN tes_tipos_doc T ON C.tipo = T.tipo
INNER JOIN tes_banco_docs Y ON C.id_banco = Y.id_Banco AND C.tipo = Y.tipo
WHERE
    C.nsolicitud = @solicitud
    AND C.estado IN ('T','E','I');";

                var data = conn.QueryFirstOrDefault<TesReposicionData>(query, new { solicitud });

                // Si no existe, devolvemos error con un objeto consistente (evita nulls)
                if (data is null)
                {
                    var notValid = new TesReposicionData
                    {
                        verificaTag = "N",
                        verifica = "Este documento no es valido para reposición..."
                    };

                    return DbHelper.CreateErrorResponse<TesReposicionData>(
                        "Este documento no es valido para reposición...",
                        -1,
                        notValid
                    );
                }

                // Base OK
                data.verificaTag = "S";
                data.verifica = "----> Este Documento se puede marcar para reponer";

                // Reglas que lo invalidan
                if (data.reposicionPaso == 1)
                {
                    data.verificaTag = "N";
                    data.verifica += " Este documento ya Registró Reposición Anteriormente!...";
                }

                // OJO: en tu código comparas con "3" string.
                // Mantengo tu lógica para no romper compatibilidad, pero si el campo es int, cámbialo a: data.tipo_Beneficiario != 3
                if (data.tipo_Beneficiario != "3")
                {
                    data.verificaTag = "N";
                    data.verifica += " - El Tipo de Beneficiario no aplica (Solo Pago de Proveedores)...";
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesReposicionData>(ex.Message, -1);
            }
        }

        /// <summary>
        /// Guarda una solicitud de reposición de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto TES_Reposicion_Guardar(int CodEmpresa, TesReposicionData data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select isnull(count(*),0) as Existe from tes_autorizaciones where nombre = @usuarios 
                                     and estado = 'A' and clave = @clave ";

                var existe = conn.QueryFirstOrDefault<int>(query, new { usuarios = data.usuario, clave = data.clave });
                if (existe == 0)
                {
                    return DbHelper.ErrorResponse("El usuario y clave de autorización no concuerda con ninguno de los registrados, verifique...", -1);
                }

                query = $@"Exec spTES_Reposicion @txtNumeroSolicitud, @glogonUsuario, @usuario, @notas";
                conn.Execute(query, new
                {
                    txtNumeroSolicitud = data.nSolicitud,
                    glogonUsuario = data.usuario,
                    usuario = data.usuario,
                    notas = data.notas
                });

                //bitácora
                mTesoreria.sbTesBitacoraEspecial(CodEmpresa, data.nSolicitud, "18", data.notas, data.usuario);

                mSecurity.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.usuario,
                    Modulo = module, // Tesoreria
                    Movimiento = "Aplica - web",
                    DetalleMovimiento = "ReImpresión de Solicitud :" + data.nSolicitud,
                });

                return DbHelper.OkResponse("La reposición se ha guardado correctamente.");

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }
    }
}
