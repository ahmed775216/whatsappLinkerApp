-- Function to clean up old sync logs
CREATE OR REPLACE FUNCTION cleanup_old_sync_logs(days_to_keep INTEGER DEFAULT 30)
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM api_sync_log 
    WHERE started_at < CURRENT_TIMESTAMP - INTERVAL '1 day' * days_to_keep;
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Function to get bot instance ID by client_id
CREATE OR REPLACE FUNCTION get_bot_instance_id(p_client_id VARCHAR)
RETURNS INTEGER AS $$
BEGIN
    RETURN (SELECT id FROM bot_instances WHERE client_id = p_client_id LIMIT 1);
END;
$$ LANGUAGE plpgsql;

-- Function to check if JID is whitelisted
CREATE OR REPLACE FUNCTION is_whitelisted(p_bot_instance_id INTEGER, p_jid VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS(
        SELECT 1 FROM whitelisted_users 
        WHERE bot_instance_id = p_bot_instance_id AND user_jid = p_jid AND api_active = true
        UNION
        SELECT 1 FROM whitelisted_groups 
        WHERE bot_instance_id = p_bot_instance_id AND group_jid = p_jid AND is_active = true
    );
END;
$$ LANGUAGE plpgsql;