public static class Queries
{
    public const string GetWhitelistedUsers = @"
        SELECT user_jid as UserJid, phone_number as PhoneNumber, 
               display_name as DisplayName, allowed_in_groups as AllowedInGroups, 
               api_contact_id as ApiContactId
        FROM whitelisted_users
        WHERE bot_instance_id = @botInstanceId AND api_active = true";
    
    public const string GetGroupParticipants = @"
        SELECT gp.*, 
               COALESCE(wu.api_active, false) as IsWhitelisted,
               wu.allowed_in_groups
        FROM group_participants gp
        LEFT JOIN whitelisted_users wu 
            ON wu.bot_instance_id = gp.bot_instance_id 
            AND wu.user_jid = gp.participant_jid
        WHERE gp.bot_instance_id = @botInstanceId 
        AND gp.group_jid = @groupJid";
}