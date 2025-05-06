using FrameHub.Model.Dto.Registration;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class DefaultRegistrationStrategy(IUnitOfWork unitOfWork) : IRegistrationStrategy
{
    public async Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        // Default Registration to be implemented
        
        // Todo : Algorithm for next time  : 
        //  1) Inject proper repositories
        //  2) Create User Entity, UserCredential, UserInfo, UserSubscription(default value)
        //  3) Save or rollback accordingly.
        //   -- Create each repo for each table ? Or have a single ? User Repository which checks all about users ?
        //      -- I lean towards logically encapsulation , meaning User repository to do anything user is concerned --> Plus less DI here.
        
        await unitOfWork.BeginTransactionAsync();
        try
        {
            // implementation...
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            // TODO : Throw here custom exception
        }

        throw new NotImplementedException();
    }
}