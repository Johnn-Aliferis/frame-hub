using FrameHub.Model.Dto.Media;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Interfaces;

public interface IMediaService
{
    Task<string> GeneratePresignedUrl(string userId, string userEmail);
    Task<string> DeleteImage(string url);
    Task<string> ConfirmMediaUploadAsync(string userId, PhotoRequestDto photoRequest); // todo :yup ,  should this return PhotoResponseDTo for front end ?
}