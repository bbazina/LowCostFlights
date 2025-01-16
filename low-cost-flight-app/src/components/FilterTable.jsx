import React from "react";
import Icon from "@mdi/react";
import { mdiAirplaneTakeoff } from "@mdi/js";
import { mdiAirplaneLanding } from "@mdi/js";
import { mdiAirplaneClock } from "@mdi/js";
import { mdiTransitConnection } from "@mdi/js";
import { mdiAccountGroup } from "@mdi/js";
import { mdiCurrencyUsd } from "@mdi/js";
import { mdiCashMultiple } from "@mdi/js";
import ClipLoader from "react-spinners/ClipLoader";

import "../styles/FlightsTable.css";

const FlightTable = ({ flights, loading }) => {
  return (
    <div className="flight-table-container">
      {loading ? (
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            marginTop: "20px",
          }}
        >
          <ClipLoader color="#4A90E2" size={50} />
        </div>
      ) : flights.length > 0 ? (
        <table>
          <thead>
            <tr>
              <th>
                <Icon
                  path={mdiAirplaneTakeoff}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Origin
              </th>
              <th>
                <Icon
                  path={mdiAirplaneLanding}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Destination
              </th>
              <th>
                <Icon
                  path={mdiAirplaneClock}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Departure Date
              </th>
              <th>
                {" "}
                <Icon
                  path={mdiAirplaneClock}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Return Date
              </th>
              <th>
                <Icon
                  path={mdiTransitConnection}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Departure Stops
              </th>
              <th>
                <Icon
                  path={mdiTransitConnection}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Return Stops
              </th>
              <th>
                <Icon
                  path={mdiAccountGroup}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Passengers
              </th>
              <th>
                <Icon
                  path={mdiCurrencyUsd}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Currency
              </th>
              <th>
                <Icon
                  path={mdiCashMultiple}
                  size={1}
                  style={{ marginRight: "5px" }}
                />
                Total Price
              </th>
            </tr>
          </thead>
          <tbody>
            {flights.map((flight, index) => (
              <tr key={index}>
                <td>{flight.originAirport}</td>
                <td>{flight.destinationAirport}</td>
                <td>{flight.departureDate}</td>
                <td>{flight.returnDate}</td>
                <td>{flight.departureNumberOfStops}</td>
                <td>{flight.returnNumberOfStops}</td>
                <td>{flight.numberOfPassengers}</td>
                <td>{flight.currency}</td>
                <td>{flight.totalPrice}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <div>No flights found</div>
      )}
    </div>
  );
};

export default FlightTable;
