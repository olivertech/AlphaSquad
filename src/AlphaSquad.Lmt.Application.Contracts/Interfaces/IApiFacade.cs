using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IApiFacade
{
    Task<DashboardConfigurationResponseDto?> GETApiConfigurationsMeDashboardAsync(CancellationToken cancellationToken = default);

    Task DELETEApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default);

    Task DELETEApiClassesByIdBookingsByBookingIdAsync(string id, Guid bookingId, CancellationToken cancellationToken = default);

    Task DELETEApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiProfileMePhotoAsync(CancellationToken cancellationToken = default);

    Task DELETEApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default);

    Task DELETEApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);

    Task DELETEApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AuthenticatedSessionResponseDto?> GETApiAuthMeAsync(CancellationToken cancellationToken = default);

    Task<List<UserWithoutRecentCheckInResponseDto>?> GETApiCheckinsInactiveUsersAsync(int? daysWithoutCheckIn, CancellationToken cancellationToken = default);

    Task<List<CheckInResponseDto>?> GETApiCheckinsMeAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<CheckInResponseDto>?> GETApiCheckinsTenantAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, Guid? userId, CancellationToken cancellationToken = default);

    Task<List<GymClassResponseDto>?> GETApiClassesAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? isActive, int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<ClassBookingManagementResponseDto>?> GETApiClassesBookingsByUserByUserIdAsync(Guid userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? onlyActiveClasses, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> GETApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ClassBookingManagementResponseDto>?> GETApiClassesByIdBookingsAsync(string id, bool? onlyActiveClasses, CancellationToken cancellationToken = default);

    Task<List<AcademyEventResponseDto>?> GETApiEventsAsync(bool? isActive, bool? onlyOutdoor, int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<AcademyEventResponseDto?> GETApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<AcademyEventParticipationResponseDto>?> GETApiEventsByIdParticipantsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task DELETEApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<List<AcademyEventResponseDto>?> GETApiEventsFeedAsync(string cursor, int? limit, bool? onlyOutdoor, CancellationToken cancellationToken = default);

    Task<List<ExerciseResponseDto>?> GETApiExercisesAsync(CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> GETApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MyGamificationDashboardResponseDto?> GETApiGamificationMeAsync(CancellationToken cancellationToken = default);

    Task<List<MonthlyRankingEntryResponseDto>?> GETApiGamificationRankingMonthlyAsync(int? month, int? top, int? year, CancellationToken cancellationToken = default);

    Task<List<GamificationEventRuleResponseDto>?> GETApiGamificationRulesAsync(CancellationToken cancellationToken = default);

    Task<List<MonthlyWinnerHistoryEntryResponseDto>?> GETApiGamificationWinnersHistoryAsync(int? limitMonths, CancellationToken cancellationToken = default);

    Task<TenantLegalContentResponseDto?> GETApiLegalCurrentAsync(CancellationToken cancellationToken = default);

    Task<List<TenantMediaResponseDto>?> GETApiMediaAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<MediaResponseDto?> GETApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<MembershipPlanResponseDto>?> GETApiPlansAsync(CancellationToken cancellationToken = default);

    Task<List<UserWithoutActivePlanResponseDto>?> GETApiPlansInactiveUsersAsync(int? daysWithoutPlan, CancellationToken cancellationToken = default);

    Task<List<UserMembershipHistoryResponseDto>?> GETApiPlansUsersByUserIdHistoryAsync(string userId, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> GETApiProfileMeAsync(CancellationToken cancellationToken = default);

    Task<List<SocialPostResponseDto>?> GETApiSocialPostsAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<SocialPostResponseDto?> GETApiSocialPostsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SocialCommentResponseDto>?> GETApiSocialPostsByIdCommentsAsync(string id, CancellationToken cancellationToken = default);

    Task<List<SocialPostResponseDto>?> GETApiSocialPostsFeedAsync(string cursor, int? limit, CancellationToken cancellationToken = default);

    Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersAsync(int? page, int? pageSize, int? status, Guid? userId, CancellationToken cancellationToken = default);

    Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersMeAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> GETApiStoreOrdersMeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProductListItemResponseDto>?> GETApiStoreProductsAsync(bool? includeInactive, int? page, int? pageSize, string search, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> GETApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProductListItemResponseDto>?> GETApiStoreProductsFeedAsync(string cursor, int? limit, string search, CancellationToken cancellationToken = default);

    Task<TenantConfigResponseDto?> GETApiTenantsBySlugBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<TenantCurrentResponseDto?> GETApiTenantsCurrentAsync(CancellationToken cancellationToken = default);

    Task<TenantFeaturesResponseDto?> GETApiTenantsCurrentFeaturesAsync(CancellationToken cancellationToken = default);

    Task<List<UserResponseDto>?> GETApiUsersAsync(CancellationToken cancellationToken = default);

    Task<UserResponseDto?> GETApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<WorkoutResponseDto>?> GETApiWorkoutsAsync(CancellationToken cancellationToken = default);

    Task<WorkoutDetailsResponseDto?> GETApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<string?> POSTApiAuthChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    Task<LoginResponseDto?> POSTApiAuthLoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiAuthLogoutAsync(CancellationToken cancellationToken = default);

    Task<LoginResponseDto?> POSTApiAuthRefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);

    Task<CheckInResponseDto?> POSTApiCheckinsAsync(CreateCheckInRequestDto request, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> POSTApiClassesAsync(CreateGymClassRequestDto request, CancellationToken cancellationToken = default);

    Task<ClassBookingResponseDto?> POSTApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default);

    Task<ClassBookingResponseDto?> POSTApiClassesByIdBookingsAsync(string id, CreateClassBookingForUserRequestDto request, CancellationToken cancellationToken = default);

    Task<AcademyEventResponseDto?> POSTApiEventsAsync(CreateAcademyEventRequestDto request, CancellationToken cancellationToken = default);

    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdCheckinAsync(Guid id, EventCheckInRequestDto request, CancellationToken cancellationToken = default);

    Task<CompleteAcademyEventResponseDto?> POSTApiEventsByIdCompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipateAsync(string id, CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> POSTApiExercisesAsync(ExerciseCreateRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiGamificationRankingMonthlyCloseAsync(CloseMonthlyRankingRequestDto request, CancellationToken cancellationToken = default);

    Task<GamificationEventRuleResponseDto?> POSTApiGamificationRulesAsync(CreateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default);

    Task<UploadMediaResponseDto?> POSTApiMediaUploadAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);

    Task<MembershipPlanResponseDto?> POSTApiPlansAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiPlansByIdAssignAsync(string id, AssignMembershipPlanRequestDto request, CancellationToken cancellationToken = default);

    Task<MembershipPaymentResponseDto?> POSTApiPlansPaymentsAsync(RecordMembershipPaymentRequestDto request, CancellationToken cancellationToken = default);

    Task<SocialResponseDto?> POSTApiSocialPostsAsync(CreateSocialPostRequestDto request, CancellationToken cancellationToken = default);

    Task<SocialCommentResponseDto?> POSTApiSocialPostsByIdCommentsAsync(string id, CreateSocialCommentRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> POSTApiStoreOrdersAsync(CreateStoreOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> POSTApiStoreProductsAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductVariantResponseDto?> POSTApiStoreProductsByIdVariantsAsync(Guid id, CreateProductVariantRequestDto request, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> POSTApiUsersAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);

    Task<WorkoutResponseDto?> POSTApiWorkoutsAsync(WorkoutCreateRequestDto request, CancellationToken cancellationToken = default);

    Task<WorkoutDetailsResponseDto?> POSTApiWorkoutsByIdExercisesAsync(string id, List<ExerciseAssignmentRequestDto> request, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> PUTApiClassesByIdAsync(Guid id, UpdateGymClassRequestDto request, CancellationToken cancellationToken = default);

    Task<DashboardConfigurationResponseDto?> PUTApiConfigurationsMeDashboardAsync(UpdateDashboardConfigurationRequestDto request, CancellationToken cancellationToken = default);

    Task<AcademyEventResponseDto?> PUTApiEventsByIdAsync(Guid id, UpdateAcademyEventRequestDto request, CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> PUTApiExercisesByIdAsync(Guid id, ExerciseUpdateRequestDto request, CancellationToken cancellationToken = default);

    Task<GamificationEventRuleResponseDto?> PUTApiGamificationRulesByIdAsync(Guid id, UpdateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default);

    Task<TenantLegalContentResponseDto?> PUTApiLegalCurrentAsync(UpdateTenantLegalContentRequestDto request, CancellationToken cancellationToken = default);

    Task<TenantMediaResponseDto?> PUTApiMediaByIdFileAsync(string id, MultipartBodyDto request, CancellationToken cancellationToken = default);

    Task<MembershipPlanResponseDto?> PUTApiPlansByIdAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMeAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMeEmailAsync(UpdateProfileEmailRequestDto request, CancellationToken cancellationToken = default);

    Task<string?> PUTApiProfileMePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMePhotoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> PUTApiStoreOrdersByIdStatusAsync(string id, UpdateStoreOrderStatusRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> PUTApiStoreProductsByIdAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductVariantResponseDto?> PUTApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default);

    Task<TenantConfigResponseDto?> PUTApiTenantsByIdAsync(Guid id, UpdateTenantRequestDto request, CancellationToken cancellationToken = default);

    Task<TenantCurrentResponseDto?> PUTApiTenantsCurrentLogoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> PUTApiUsersByIdAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);

    Task<WorkoutResponseDto?> PUTApiWorkoutsByIdAsync(Guid id, WorkoutUpdateRequestDto request, CancellationToken cancellationToken = default);
}
