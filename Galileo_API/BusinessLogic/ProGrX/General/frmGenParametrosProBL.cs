using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmGenParametrosProBL
    {
        readonly FrmGenParametrosProDb DbParametrosPro;

        public FrmGenParametrosProBL(IConfiguration config)
        {
            DbParametrosPro = new FrmGenParametrosProDb(config);
        }

        /// <summary>
        /// Obtiene los parametros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<PvParametrosModDto> Obtener_ParamaterosPro(int CodEmpresa)
        {
            return DbParametrosPro.Obtener_ParamaterosPro(CodEmpresa);
        }

        /// <summary>
        /// Actualiza los parametros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaGen(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            return DbParametrosPro.ParamaterosPro_ActualizaGen(CodEmpresa, pvParametrosMod);
        }

        /// <summary>
        /// Actualiza los parametros de CxP
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaCxP(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            return DbParametrosPro.ParamaterosPro_ActualizaCxP(CodEmpresa, pvParametrosMod);
        }

        /// <summary>
        /// Actualiza los parametros de Inventario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaInv(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            return DbParametrosPro.ParamaterosPro_ActualizaInv(CodEmpresa, pvParametrosMod);

        }

        /// <summary>
        /// Actualiza los parametros de POS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaPos(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            return DbParametrosPro.ParamaterosPro_ActualizaPos(CodEmpresa, pvParametrosMod);

        }

        public ErrorDto ParametrosGen_Insertar(int CodEmpresa)
        {
            return DbParametrosPro.ParametrosGen_Insertar(CodEmpresa);
        }
    }
}
