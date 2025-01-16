import React, { useState } from "react";
import axios from "axios";
import Icon from "@mdi/react";
import { mdiAirplaneSearch } from "@mdi/js";
import Filter from "./components/Filter";
import FlightTable from "./components/FilterTable";
import { currencyMap } from "./utils/currency";

const App = () => {
  const [flights, setFlights] = useState([]);
  const [loading, setLoading] = useState(false);

  const fetchFlights = async (filterData) => {
    try {
      setLoading(true);
      const response = await axios.get(`https://localhost:44357/api/Flight`, {
        params: filterData,
      });
      const transformedFlights = response.data.map((flight) => ({
        ...flight,
        currency: currencyMap[flight.currency] || "Unknown",
      }));
      setFlights(transformedFlights);
    } catch (error) {
      console.error("Error fetching flights:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <header>
        <h1>
          <Icon
            path={mdiAirplaneSearch}
            size={1.5}
            style={{ marginRight: "10px" }}
          />
          Low-Cost Flights Search
        </h1>
        <p>Find the best deals on your next flight!</p>
      </header>
      <Filter onSearch={fetchFlights} />
      <FlightTable flights={flights} loading={loading} />
    </div>
  );
};

export default App;
