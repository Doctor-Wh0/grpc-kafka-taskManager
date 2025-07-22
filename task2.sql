
CREATE TABLE client_payments (
    id BIGSERIAL PRIMARY KEY,
    client_id BIGINT NOT NULL,
    dt TIMESTAMP NOT NULL,
    amount NUMERIC(10, 2) NOT NULL
);

-- Наполнение таблицы случайными данными
INSERT INTO client_payments (id, client_id, dt, amount) VALUES
(1, 1, '2022-01-03 17:24:00', 100),
(2, 1, '2022-01-05 17:24:14', 200),
(3, 1, '2022-01-05 18:23:34', 250),
(4, 1, '2022-01-07 10:12:38', 50),
(5, 2, '2022-01-05 17:24:14', 278),
(6, 2, '2022-01-10 12:39:29', 300);


-- Проверка данных
SELECT * FROM client_payments;


CREATE OR REPLACE FUNCTION get_daily_payments(client INT, start_date DATE, end_date DATE)
RETURNS TABLE(payment_date DATE, daily_amount NUMERIC) AS $$
BEGIN
    RETURN QUERY
    SELECT
        CAST(gs AS DATE) AS payment_date, -- Приведение TIMESTAMP к DATE
        COALESCE(SUM(cp.amount), 0) AS daily_amount
    FROM
        generate_series(start_date, end_date, '1 day'::interval) AS gs
    LEFT JOIN
        client_payments cp ON CAST(cp.dt AS DATE) = CAST(gs AS DATE) AND cp.client_id = client
    GROUP BY
        CAST(gs AS DATE)
    ORDER BY
        CAST(gs AS DATE);
END;
$$ LANGUAGE plpgsql;





SELECT payment_date, daily_amount FROM get_daily_payments(1, '2022-01-02', '2022-01-07');