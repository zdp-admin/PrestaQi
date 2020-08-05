namespace PrestaQi.Model.Enum
{
    public class PrestaQiEnum
    {
        public enum CapitalEnum
        {
            Solicitado = 1,
            Enviado = 2,
            Terminado = 3
        }

        public enum InvestmentEnum
        {
            Activa = 1,
            NoActiva = 2,
            Terminada = 3
        }

        public enum PerdioAccredited
        {
            Semanal = 7,
            Quincenal = 9,
            Mensual = 10
        }

        public enum AdvanceStatus
        {
            NoPagado = 0,
            Pagado = 1,
            PagadoParcial = 2
        }

        public enum UserType
        {
            Administrador = 1,
            Inversionista = 2,
            Acreditado = 3
        }

        public enum NotificationType
        {
            ChangePassword = 1,
            SetPaymentPeriod,
            CapitalCall,
            AdvanceStatus,
            ChangeStatusCapital,
            SetPaymentAdvance,
            PaymentInterest,
            DeleteUser,
            RemoveUser
        }

        public enum NotificationIconType
        {
           done = 1,
           warning = 2,
           info = 3,
           error = 4
        }
    }
}
